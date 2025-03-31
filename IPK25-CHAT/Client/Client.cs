/* Contains an abstract class for a client. */

namespace IPK_25_CHAT.Client;

using System.Net;
using System.Net.Sockets;
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
    /// Socket to send from and recieve to.
    /// </summary>
    protected Socket ClientSocket;

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
    /// Throws events subscribed to by the client on receiving server input.
    /// </summary>
    private readonly IServerInputReader ServerInputReader;

    /// <summary>
    /// Queue of user inputs.
    /// </summary>
    private readonly UserInputQueue InputQueue = new();

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
    public Client(IPAddress host, ushort port)
    {
        // Instance attributes
        Host = host;
        Port = port;
        ServerInputReader = CreateServerInputReader();
        ServerOutputWriter = CreateServerOutputWriter();
        ClientSocket = CreateSocket();
        InputValidator = new UserInputValidator(this);

        // Subscribe to events
        UserInputReader.UserInputReceived += OnUserInputReceived;
        ServerInputReader.ErrMessageReceived += OnErrMessageReceived;
        ServerInputReader.ByeMessageReceived += OnByeMessageReceived;
    }

    /// <summary>
    /// Called after the UserInputReceived event is raised in the UserInputReader class.
    /// Enqueues the given input.
    /// </summary>
    /// <param name="input">The given input from the user.</param>
    private void OnUserInputReceived(string input) => InputQueue.Enqueue(input);

    /// <summary>
    /// Used to send messages to the server.
    /// </summary>
    private IServerOutputWriter ServerOutputWriter;

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
            }
        } while(State != State.END);

        EndState();
    }

    /// <summary>
    /// Handles the client's starting state.
    /// </summary>
    private async Task StartState()
    {
        try
        {
            // Get the next user input
            IReadable? input = InputValidator.Validate(await InputQueue.Dequeue());

            // If the input is null, the user input was invalid
            if(input == null) return;

            // If the input is a valid command (auth or help for now), execute it
            else
            {
                Command command = (Command)input;
                ExecuteCommand(command);
            }
        }

        // Task over
        catch(OperationCanceledException)
        {
            return;
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
        ServerInputReader.Close();
        ServerOutputWriter.Close();
        UserInputReader.Close();
        Environment.Exit(0);
    }

    /// <summary>
    /// Creates the socket for sending and receiving messages.
    /// </summary>
    /// <returns>Initialized socket object with (AddressFamily.InterNetwork, SockType.Stream|Dgram, ProtocolType.Tcp|Udp)</returns>
    protected abstract Socket CreateSocket();

    /// <summary>
    /// Creates the server input reader for the given type of the Client (TCP/UDP).
    /// </summary>
    protected abstract IServerInputReader CreateServerInputReader();

    /// <summary>
    /// Creates the server output writer for the given type of the Client (TCP/UDP).
    /// </summary>
    protected abstract IServerOutputWriter CreateServerOutputWriter();
}