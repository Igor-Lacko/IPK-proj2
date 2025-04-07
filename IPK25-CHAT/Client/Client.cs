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
    protected State ClientState = State.START;

    /// <summary>
    /// IP address of the server.
    /// </summary>
    protected IPAddress Host;

    /// <summary>
    /// Port number of the server.
    /// </summary>
    protected ushort Port;

    /// <summary>
    /// Current session settings (username, etc.).
    /// </summary>
    protected UserSessionConfiguration Config = new();

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
    /// Stores the current message from the server.
    /// </summary>
    protected readonly ServerInputStorage MessageStorage = new();

    /// <summary>
    /// Validator for user input.
    /// </summary>
    protected readonly UserInputValidator InputValidator;

    /// <summary>
    /// Currently received message from the server.
    /// </summary>
    protected Message? ReceivedMessage = null;

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
        ServerCommunicator.MessageReceived += MessageStorage.OnMessageReceived;

        // Don't immediately stop running
        Console.CancelKeyPress += (sender, e) => {  e.Cancel = true; UserInputQueue.Enqueue(null); };
    }

    /// <summary>
    /// Called when the end of file (EOF) is received, (well actually dequeued).
    /// Also called on CTRL + C (when null is dequeued).
    /// </summary>
    protected async Task OnEofReceived()
    {
        if(ClientState == State.START) Environment.Exit(0);
        else if (ClientState != State.END)
            await ServerCommunicator.SendMessage(new ByeMessage(Config.DisplayName!));
        GracefulTermination();
        Environment.Exit(0);
    }

    /// <summary>
    /// Updates the client state in the client and the validator.
    /// </summary>
    /// <param name="state">New state.</param>
    protected void UpdateState(State state)
    {
        // Set the state
        ClientState = state;

        // Update the validator
        InputValidator.ClientState = state;
    }

    /// <summary>
    /// Updates the username in the client and the validator.
    /// </summary>
    /// <param name="username">New username.</param>
    protected void UpdateUsername(string username)
    {
        // Set the username
        Config.Username = username;

        // Update the validator
        InputValidator.Config.Username = username;
    }

    /// <summary>
    /// Updates the display name in the client and the validator.
    /// </summary>
    /// <param name="displayName">New display name.</param>
    protected void UpdateDisplayName(string displayName)
    {
        // Set the display name
        Config.DisplayName = displayName;

        // Update the validator
        InputValidator.Config.DisplayName = displayName;
    }

    /// <summary>
    /// Updates the channel ID in the client and the validator.
    /// </summary>
    /// <param name="channelID">New channel ID.</param>
    protected void UpdateChannelID(string channelID)
    {
        // Set the channel ID
        Config.ChannelID = channelID;

        // Update the validator
        InputValidator.Config.ChannelID = channelID;
    }

    /// <summary>
    /// Updates the requested channel ID in the client and the validator.
    /// </summary>
    /// <param name="channelID">New requested channel ID.</param>
    protected void UpdateRequestedChannelID(string? channelID)
    {
        // Set the requested channel ID
        Config.RequestedChannelID = channelID;

        // Update the validator
        InputValidator.Config.RequestedChannelID = channelID;
    }

    /// <summary>
    /// Sends a message or executes a command.
    /// </summary>
    /// <param name="input">Message or command.</param>
    protected async Task RunUserInput(IReadable input)
    {
        if(input is Command command)
            await ExecuteCommand(command);

        else if(input is Message message)
            await ServerCommunicator.SendMessage(message);
    }

    /// <summary>
    /// Called after the UserInputReceived event is raised in the UserInputReader class.
    /// Enqueues the given input.
    /// </summary>
    /// <param name="input">The given input from the user.</param>
    private void OnUserInputReceived(string? input) => UserInputQueue.Enqueue(input);

    /// <summary>
    /// Called to check if a message from the server is terminating the connection.
    /// </summary>
    /// <param name="message">Message received.</param>
    /// <returns>True if the message is terminating, false otherwise.</returns>
    protected abstract Task TerminatingMessageReceived(Message message);

    /// <summary>
    /// Executes the given AUTH command.
    /// </summary>
    /// <param name="command">AUTH command with parameters.</param>
    protected abstract Task ExecuteAuthCommand(AuthCommand command);

    /// <summary>
    /// Executes the given JOIN command.
    /// </summary>
    /// <param name="command">JOIN command with parameters.</param>
    protected abstract Task ExecuteJoinCommand(JoinCommand command);

    /// <summary>
    /// Executes the given command.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    protected async Task ExecuteCommand(Command command)
    {
        // Decide based on the type
        switch(command.Type)
        {
            case CommandType.AUTH:
                await ExecuteAuthCommand((AuthCommand)command);
                break;

            case CommandType.HELP:
                StdoutResultWriter.PrintCommands();
                break;

            case CommandType.RENAME:
                UpdateDisplayName(((RenameCommand)command).DisplayName);
                break;

            case CommandType.JOIN:
                await ExecuteJoinCommand((JoinCommand)command);
                break;

            case CommandType.STATUS:
                StatusCommand.PrintStatus(Config, Port, Host, ClientState);
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
        // Wait for the user to type in a command
        string? input = await UserInputQueue.Dequeue();
        if(input == null) await OnEofReceived();

        // Validate the input (all commands except join are valid in this state (and except a message))
        IReadable? validatedInput = InputValidator.Validate(input!);

        // Run or stay in start
        if(validatedInput != null) await RunUserInput(validatedInput);
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
    protected abstract Task ErrorExit(bool sendErrorMessage, string? errorMessage, bool terminateConnection, int exitCode = 1);
}