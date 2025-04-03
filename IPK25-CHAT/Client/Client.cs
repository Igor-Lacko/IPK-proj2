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
    /// Field to allow non-recursive setting.
    /// </summary>
    protected State _state = State.START;

    /// <summary>
    /// Client's current state.
    /// </summary>
    protected State ClientState
    {
        get => _state;
        set
        {
            _state = value;
            InputValidator.ClientState = value;
        }
    }

    /// <summary>
    /// IP address of the server.
    /// </summary>
    protected IPAddress Host;

    /// <summary>
    /// Port number of the server.
    /// </summary>
    protected ushort Port;

    /// <summary>
    /// Field to allow non-recursive setting.
    /// </summary>
    protected UserSessionConfiguration _config = new();

    /// <summary>
    /// Current session settings (username, etc.).
    /// </summary>
    protected UserSessionConfiguration Config
    {
        get => _config;
        set
        {
            _config = value;
            InputValidator.Config = value;
        }
    }

    /// <summary>
    /// Throws events subscribed to by the client on receiving user input.
    /// </summary>
    protected readonly UserInputReader InputReader = new();

    /// <summary>
    /// Reads input from the server and sends messages to the server.
    /// </summary>
    protected readonly IServerCommunicator ServerCommunicator;

    /// <summary>
    /// Queue of user inputs.
    /// </summary>
    protected readonly InputQueue<string?> UserInputQueue = new();

    /// <summary>
    /// Queue of server inputs. Might not be needed? TODO.
    /// </summary>
    protected readonly InputQueue<Message> ServerInputQueue = new();

    /// <summary>
    /// Validator for user input.
    /// </summary>
    protected readonly UserInputValidator InputValidator;

    /// <summary>
    /// Constructor. Sets the host and port.
    /// </summary>
    /// <param name="host">IP address of the server.</param>
    /// <param name="port">Port number of the server.</param>
    public Client(IPAddress host, ushort port)
    {
        // Instance attributes
        Host = host;
        Port = port;
        ServerCommunicator = CreateServerCommunicator();
        InputValidator = new UserInputValidator();

        // Subscribe to events
        InputReader.UserInputReceived += OnUserInputReceived;
        ServerCommunicator.MessageReceived += OnServerInputReceived;

        // Don't immediately stop running
        Console.CancelKeyPress += (sender, e) => {  e.Cancel = true; UserInputQueue.Enqueue(null); };
    }

    /// <summary>
    /// Called when the end of file (EOF) is received, (well actually dequeued).
    /// Also called on CTRL + C (when null is dequeued).
    /// </summary>
    protected void OnEofReceived()
    {
        if (ClientState != State.END && ClientState != State.START)
            ServerCommunicator.SendMessage(new ByeMessage(Config.DisplayName!));
        GracefulTermination();
        Environment.Exit(0);
    }

    /// <summary>
    /// Sends a message or executes a command.
    /// </summary>
    /// <param name="input">Message or command.</param>
    protected void RunUserInput(IReadable input)
    {
        if(input is Command command)
            ExecuteCommand(command);

        else if(input is Message message)
            ServerCommunicator.SendMessage(message);
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
    private void OnServerInputReceived(Message message) => ServerInputQueue.Enqueue(message);

    /// <summary>
    /// Called to check if a message from the server is terminating the connection.
    /// </summary>
    /// <param name="message">Message received.</param>
    /// <returns>True if the message is terminating, false otherwise.</returns>
    protected abstract bool TerminatingMessageReceived(Message message);

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
    protected void ExecuteCommand(Command command)
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
    public async Task Run()
    {
        // Receive input
        InputReader.Run();
        ServerCommunicator.Run();

        // FSM loop
        do
        {
            switch(ClientState)
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
            } 
        } while(ClientState != State.END);

        EndState();
    }

    /// <summary>
    /// Handles the client's starting state.
    /// </summary>
    private async Task StartState()
    {
        // Cancellation token source to only wait for one task
        using var readInputCancel = new CancellationTokenSource();

        // Tasks for user/server input
        Task<string?> userInputTask = UserInputQueue.Dequeue(readInputCancel.Token);
        Task<Message> serverInputTask = ServerInputQueue.Dequeue(readInputCancel.Token);

        // Wait for either task to finish
        Task finishedTask = await Task.WhenAny(userInputTask, serverInputTask);

        // Cancel the other task
        readInputCancel.Cancel();

        // Check which task finished
        if(finishedTask == userInputTask)
        {
            // EOF or CTRL + C
            if(userInputTask.Result == null) OnEofReceived();

            // Get the user input
            IReadable? input = InputValidator.Validate(userInputTask.Result!);
            if(input == null) return;
            else RunUserInput(input);
        }

        else if (finishedTask == serverInputTask)
        {
            // Get the server input
            Message message = serverInputTask.Result;

            // Local client error, terminate connection and the application
            if(message.Type == MessageType.MALFORMED)
            {
                StdoutResultWriter.InternalClientError(((MalformedMessage)message).MessageContent);
                ErrorExit(false, null, false);
            }

            else if(message.Type == MessageType.ERR || message.Type == MessageType.BYE)
            {
                ClientState = State.END;
                return;
            }

            // Again, local client error, terminate connection and the application
            else
            {
                StdoutResultWriter.InternalClientError(message.ToString());
                ErrorExit(false, null, false);
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
    protected abstract void EndState();

    /// <summary>
    /// Creates the protocol-specific server communicator.
    /// </summary>
    /// <returns>Server communicator object.</returns>
    protected abstract IServerCommunicator CreateServerCommunicator();

    /// <summary>
    /// Gracefully terminates the connection to the server.
    /// </summary>
    protected abstract void GracefulTermination();

    /// <summary>
    /// Triggered on the error states of the client.
    ///     - Send a ERR message to the server, IF POSSIBLE.
    ///     - Gracefully terminate the connection, IF POSSIBLE.
    ///     - Exit with the given code.
    /// </summary>
    /// <param name="sendErrorMessage">Whether to send an ERR message to the server.</param>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="terminateConnection">Whether to terminate the connection.</param>
    /// <param name="exitCode">Exit code.</param>
    protected abstract void ErrorExit(bool sendErrorMessage, string? errorMessage, bool terminateConnection, int exitCode = 1);
}