/* Contains an abstract class for a client. */

namespace IPK_25_CHAT.Client;

using System.Net;
using IPK_25_CHAT.IO;
using IPK_25_CHAT.Command;
using IPK_25_CHAT.Message;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Enum;


/// <summary>
/// Abstract base class representing a client instance.
/// </summary>
public abstract class Client
{
    /// <summary>
    /// Client's current state.
    /// </summary>
    protected State State = State.START;

    /// <summary>
    /// IP address of the server.
    /// </summary>
    protected IPAddress Host;

    /// <summary>
    /// Port number of the server.
    /// </summary>
    protected ushort Port;

    /// <summary>
    /// Thrown by the client when it's state changes. Useful for example
    /// for the input validator to be able to react to invalid messages
    /// for a given state, e.g. they would otherwise be valid.
    /// </summary>
    public event Action<State> StateChanged = state => { };

    /// <summary>
    /// Thrown by the client when thje user settings (e.g. username, display name change).
    /// Used by the input validator for constructing Msg messages.
    /// </summary>
    public event Action<UserSessionConfiguration> UserSessionChanged = config => { };

    /// <summary>
    /// Current session settings (username, etc.).
    /// </summary>
    private UserSessionConfiguration Config = new();

    /// <summary>
    /// Throws events subscribed to by the client on receiving user input.
    /// </summary>
    private readonly UserInputReader UserInputReader = new();

    /// <summary>
    /// Reads input from the server and sends messages to the server.
    /// </summary>
    private readonly IServerCommunicator ServerCommunicator;

    /// <summary>
    /// Queue of user inputs.
    /// </summary>
    private readonly InputQueue<string> UserInputQueue = new();

    /// <summary>
    /// Queue of server inputs. Might not be needed? TODO.
    /// </summary>
    private readonly InputQueue<Message> ServerInputQueue = new();

    /// <summary>
    /// Cancellation token for the Run() task.
    /// </summary>
    private readonly CancellationTokenSource RunCancelToken = new();

    /// <summary>
    /// Validator for user input.
    /// </summary>
    private readonly UserInputValidator InputValidator;

    /// <summary>
    /// Constructor. Sets the host and port.
    /// </summary>
    /// <param name="host">IP address of the server.</param>
    /// <param name="port">Port number of the server.</param>
    /// <param name="T">Type of the client data (string/bytes).</param>
    public Client(IPAddress host, ushort port, Type T)
    {
        // Instance attributes
        Host = host;
        Port = port;
        ServerCommunicator = CreateServerCommunicator();
        InputValidator = new UserInputValidator(this);

        // Subscribe to the user input event
        UserInputReader.UserInputReceived += OnUserInputReceived;
    }

    /// <summary>
    /// Called after the UserInputReceived event is raised in the UserInputReader class.
    /// Enqueues the given input.
    /// </summary>
    /// <param name="input">The given input from the user.</param>
    private void OnUserInputReceived(string? input) => UserInputQueue.Enqueue(input);

    /// <summary>
    /// Called after the MessageReceived event is raised in the ServerCommunicator class.
    /// Enqueues the given message.
    /// </summary>
    private void OnServerInputReceived(Message? message) => ServerInputQueue.Enqueue(message);

    /// <summary>
    /// Client reaction to receiving a ERR message from the server.
    /// </summary>
    /// <param name="message">The ERR message.</param>
    private void OnErrMessageReceived(ErrMessage message)
    {
        // Print the message locally
        StdoutResultWriter.PrintErrMessage(message);

        // Change the state to END and raise an event
        State = State.END;
        StateChanged.Invoke(State);
    }

    /// <summary>
    /// Client reaction to receiving a BYE message from the server.
    /// </summary>
    private void OnByeMessageReceived()
    {
        // Change the state to END and raise an event
        State = State.END;
        StateChanged.Invoke(State);
    }

    /// <summary>
    /// Executes the given AUTH command.
    /// </summary>
    /// <param name="command">AUTH command with parameters.</param>
    protected abstract void ExecuteAuthCommand(AuthCommand command);

    /// <summary>
    /// Executes the given JOIN command.
    /// </summary>
    /// <param name="command">JOIN command with parameters.</param>
    protected abstract void ExecuteJoinCommand(JoinCommand command);

    /// <summary>
    /// Executes the given command.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    private void ExecuteCommand(Command command)
    {
        // Decide based on the type
        switch(command.Type)
        {
            case CommandType.AUTH:
                ExecuteAuthCommand((AuthCommand)command);
                break;

            case CommandType.HELP:
                StdoutResultWriter.PrintCommands();
                break;

            case CommandType.RENAME:
                Config.DisplayName = ((RenameCommand)command).DisplayName;
                UserSessionChanged.Invoke(Config);
                break;

            case CommandType.JOIN:
                ExecuteJoinCommand((JoinCommand)command);
                break;
        }
    }

    /// <summary>
    /// Main method. Runs the client. Is implemented as a async do-while loop which
    /// handles the current state.
    /// </summary>
    private async Task Run()
    {
        do
        {
            switch(State)
            {
                case State.START:
                    await StartState();
                    break;

                case State.AUTH:
                    await AuthState();
                    break;

                case State.OPEN:
                    await OpenState();
                    break;

                case State.JOIN:
                    await JoinState();
                    break;
            } return;
        } while(State != State.END);

        EndState();
    }

    /// <summary>
    /// Handles the client's starting state.
    /// </summary>
    private async Task StartState()
    {
        // Tasks for user/server input
        Task<string?> userInputTask = UserInputQueue.Dequeue();
        Task<Message?> serverInputTask = ServerCommunicator.ReadInput();

        // Wait for either task to finish
        Task finishedTask = await Task.WhenAny(userInputTask, serverInputTask);

        // Check which task finished
        if(finishedTask == userInputTask)
        {
            // Get the user input
            IReadable? input = InputValidator.Validate(userInputTask.Result, out bool isEOF);
            if(isEOF)
            {
                // End the program
                StateChanged.Invoke(State);
                State = State.END;
                EndState();
                return;
            }

            // Check if the input is null, if yes it's a invalid input
            else if(input == null) return;

            // Command
            else 
            {
                Command command = (Command)input;

                if(command.Type == CommandType.AUTH)
                {
                    // Change the state to AUTH and raise an event
                    State = State.AUTH;
                    StateChanged.Invoke(State);
                }

                ExecuteCommand(command);
            }
        }

        else if (finishedTask == serverInputTask)
        {
            // Get the server input
            Message? message = serverInputTask.Result;

            // Local client error, terminate connection and the application
            if(message == null)
            {
                StdoutResultWriter.InternalClientError($"ERROR: {message}");
                GracefulTermination();
                Environment.Exit(1);
            }

            if(message.Type == MessageType.ERR || message.Type == MessageType.BYE)
            {
                State = State.END;
                StateChanged.Invoke(State);
                return;
            }

            // Again, local client error, terminate connection and the application
            else
            {
                StdoutResultWriter.InternalClientError($"ERROR: {message}");
                GracefulTermination();
                Environment.Exit(1);
            }
        }
    }

    /// <summary>
    /// Handles the client's authentication state.
    /// </summary>
    protected abstract Task AuthState();

    /// <summary>
    /// Handles the client's open state.
    /// </summary>
    protected abstract Task OpenState();

    /// <summary>
    /// Handles the client's join state.
    /// </summary>
    protected abstract Task JoinState();

    /// <summary>
    /// Handles the client's end state.
    /// Might not need to be async?
    /// </summary>
    private void EndState()
    {
        ServerCommunicator.Close();
        UserInputReader.Close();
        Environment.Exit(0);
    }

    /// <summary>
    /// Creates the protocol-specific server communicator.
    /// </summary>
    /// <returns>Server communicator object.</returns>
    protected abstract IServerCommunicator CreateServerCommunicator();

    /// <summary>
    /// Gracefully terminates the connection to the server.
    /// </summary>
    protected abstract void GracefulTermination();
}