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
    protected IServerCommunicator ServerCommunicator;

    /// <summary>
    /// Queue of user inputs.
    /// </summary>
    protected readonly InputQueue<string?> UserInputQueue = new();

    /// <summary>
    /// Queue of server messages. This is probably not necessary, but it is here
    /// in case that multiple messages were sent so quicly after each other that we would
    /// "drop" one.
    /// </summary>
    protected readonly InputQueue<Message> ServerInputQueue = new();

    /// <summary>
    /// Validator for user input.
    /// </summary>
    protected readonly UserInputValidator InputValidator = new();

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
        ServerCommunicator = null!;

        // Subscribe to events
        InputReader.UserInputReceived += OnUserInputReceived;

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
    protected virtual async Task RunUserInput(IReadable input)
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
    protected async Task<bool> TerminatingMessageReceived(Message message)
    {
        // Invalid message for the given state
        if(!message.IsValid(ClientState))
            await ErrorExit(true, $"Invalid message type {message.Type} in state {ClientState}", true, 1);

        // Maybe ERR/BYE/MALFORMED
        switch(message.Type)
        {
            // Print the message and go to END
            case MessageType.ERR:
                StdoutResultWriter.PrintErrMessage((ErrMessage)message);
                await ErrorExit(false, null, true, 1);
                return true;

            // Go to END
            case MessageType.BYE:
                UpdateState(State.END);
                return true;

            // Print a local error and terminate
            case MessageType.MALFORMED:
                StdoutResultWriter.InternalClientError(((MalformedMessage)message).MessageContent);
                await ErrorExit(true, "Malformed message received", true, 1);
                return true;
        }

        // Valid message
        return false;
    }

    /// <summary>
    /// Executes the AUTH command.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    protected virtual async Task ExecuteAuthCommand(AuthCommand command)
    {
        // Set user parameters
        UpdateUsername(command.Username);
        UpdateDisplayName(command.DisplayName);

        // Send the AUTH message to the server
        await ServerCommunicator.SendMessage(new AuthMessage(command));
        UpdateState(State.AUTH);
    }

    /// <summary>
    /// Executes the given JOIN command.
    /// </summary>
    /// <param name="command">JOIN command with parameters.</param>
    protected virtual async Task ExecuteJoinCommand(JoinCommand command)
    {
        await ServerCommunicator.SendMessage(new JoinMessage(Config.DisplayName!, command.ChannelId));
        UpdateRequestedChannelID(command.ChannelId);
        UpdateState(State.JOIN);
    }

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
        // To cancel the non-finished task
        using var readInputCancel = new CancellationTokenSource();

        // Wait for the user or the server
        Task<string?> userInputTask = UserInputQueue.Dequeue(readInputCancel.Token);
        Task<Message> serverInputTask = ServerInputQueue.Dequeue(readInputCancel.Token);
        Task completedTask = await Task.WhenAny(userInputTask, serverInputTask);

        // Cancel the task that did not finish
        readInputCancel.Cancel();

        // User input came first
        if(completedTask == userInputTask)
        {
            // User input
            string? input = userInputTask.Result;

            // EOF or CTRL + C
            if(input == null) await OnEofReceived();

            // Execute if valid
            IReadable? validatedInput = InputValidator.Validate(input!);
            if(validatedInput == null) return;
            else await RunUserInput(validatedInput);
        }

        // Server sent a message first
        else
        {
            Message message = serverInputTask.Result;

            // Basically always except maybe PING in UDP, but the server only sends that after authentication anyway (i think?...)
            if(await TerminatingMessageReceived(message))
                return;
        }
    }

    /// <summary>
    /// Handles the client's authentication state.
    /// </summary>
    private async Task AuthState()
    {
        // Wait for a message from the server or a timeout
        Task timeoutTask = Task.Delay(5000);
        Task<Message> serverReplyTask = ServerInputQueue.Dequeue(CancellationToken.None);
        Task completedTask = await Task.WhenAny(serverReplyTask, timeoutTask);

        // Check if the timeout task completed first
        if(completedTask == timeoutTask)
        {
            StdoutResultWriter.InternalClientError("Timeout when waiting for reply to authentication");
            await ErrorExit(true, "Timeout when waiting for reply to authentication", true, 1);
            return;
        }

        Message reply = serverReplyTask.Result;

        // Decide based on the type
        if(await TerminatingMessageReceived(reply))
            return;

        // Check if the server replied with a positive or negative reply
        else if(reply.Type == MessageType.REPLY)
        {
            // Print the result
            StdoutResultWriter.PrintReplyMessage((ReplyMessage)reply);

            // Go to OPEN or stay
            if(((ReplyMessage)reply).OK)
            {
                UpdateState(State.OPEN);
                return;
            }
        }

        // If the server didn't reply, wait for a user command again (or a message from the server, but that would result in an error)

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
            if(userInputTask.Result == null) await OnEofReceived();

            // Get the user input
            IReadable? input = InputValidator.Validate(userInputTask.Result!);
            if(input == null) return;
            else await RunUserInput(input);
        }

        else if (finishedTask == serverInputTask)
        {
            // Get the server input
            Message message = serverInputTask.Result;

            // Check if the message is terminating, basically all messages are terminating in this state
            if(await TerminatingMessageReceived(message))
                return;

            // Reply is valid in this state, but not when waiting for the user to ask for authentication
            else if(message.Type == MessageType.REPLY)
                await ErrorExit(true, "Reply message received when not waiting for it!", true, 1);
        }
    }

    /// <summary>
    /// Handles the client's open state.
    /// </summary>
    private async Task OpenState()
    {
        // Loop while we are receivinf messages
        while(ClientState == State.OPEN)
        {
            // To cancel the non-finished task
            using var cts = new CancellationTokenSource();

            // Wait for a message from the server or user input
            Task<string?> userInputTask = UserInputQueue.Dequeue(cts.Token);
            Task<Message> serverInputTask = ServerInputQueue.Dequeue(cts.Token);

            // Wait for the first task to complete
            Task completedTask = await Task.WhenAny(serverInputTask, userInputTask);

            // Cancel the other task
            cts.Cancel();

            // Server input came first --> either a message or a terminating message
            if(completedTask == serverInputTask)
            {
                Message message = serverInputTask.Result;
                if(await TerminatingMessageReceived(message))
                    return;

                // Received MSG
                else
                {
                    StdoutResultWriter.PrintMsgMessage((MsgMessage)message);
                    continue;
                }
            }

            // User input came first --> either a message, or a valid command (basically all except auth)
            else
            {
                // Check for EOF
                if(userInputTask.Result == null) await OnEofReceived();

                // Validate the input
                IReadable? input = InputValidator.Validate(userInputTask.Result!);

                // Invalid input
                if(input == null) return;

                // Valid input
                else await RunUserInput(input);
            }
        }
    }

    /// <summary>
    /// Handles the client's join state.
    /// </summary>
    private async Task JoinState()
    {
        /* Stay in this state until:
            - REPLY message is received --> go to OPEN regardless of the result
            - Terminating message is received --> go to END or exit the program (TerminatingMessageReceived() handles these cases)
            - Waiting for a reply timeouts --> Exit the program
        - If a normal (MSG) message is received, print it and stay in this state
        */

        // Task representing the timeout
        Task timeoutTask = Task.Delay(5000);

        // Task waiting for a reply
        Task replyTask = WaitForReplyJoin();

        // First one of these
        Task completedTask = await Task.WhenAny(timeoutTask, replyTask);

        // If the timeout task completed first
        if(completedTask == timeoutTask)
        {
            StdoutResultWriter.InternalClientError("Timeout when waiting for reply to JOIN");
            await ErrorExit(true, "Timeout when waiting for reply to JOIN", true, 1);
        }
    }

    /// <summary>
    /// Handles the client's end state.
    /// </summary>
    protected void EndState()
    {
        GracefulTermination();
        Environment.Exit(0);
    }


    /// <summary>
    /// Waits for a reply message from the server in the JOIN state.
    /// Needed as a separate task, since in this state other messages are valid when waiting (as opposed to AUTH).
    /// Here we need to process MSG messages, while in auth if another message came when waiting
    /// it is invalid, and can be simply handled by stopping waiting and terminating the program.
    /// </summary>
    private async Task WaitForReplyJoin()
    {
        while(true)
        {
            // Wait for a message from the server
            Message message = await ServerInputQueue.Dequeue(CancellationToken.None);

            // If it's a REPLY
            if(message.Type == MessageType.REPLY)
            {
                // Update current and requested channel ID
                UpdateChannelID(Config.RequestedChannelID!);
                UpdateRequestedChannelID(null);

                // Update state
                UpdateState(State.OPEN);

                // Print the message
                StdoutResultWriter.PrintReplyMessage((ReplyMessage)message);
                return;
            }

            // Normal message, print and stay waiting
            else if(message.Type == MessageType.MSG)
                StdoutResultWriter.PrintMsgMessage((MsgMessage)message);

            // Terminating message case
            else if(await TerminatingMessageReceived(message))
                return;
        }
    }

    /// <summary>
    /// Creates the protocol-specific server communicator.
    /// </summary>
    /// <returns>Server communicator object.</returns>
    protected abstract IServerCommunicator CreateServerCommunicator();

    /// <summary>
    /// Gracefully terminates the connection to the server.
    /// </summary>
    protected void GracefulTermination()
    {
        // Close the server communicator
        ServerCommunicator.Close();

        // Close the input reader
        InputReader.Close();
    }

    /// <summary>
    /// Triggered on the error states of the client.
    ///     - Send a ERR message to the server, IF POSSIBLE.
    ///     - Gracefully terminate the connection, IF POSSIBLE.
    ///     - Exit with the given code.
    /// </summary>
    /// <param name="sendErrorMessage">Whether to send an ERR message to the server.</param>
    /// <param name="errorMessage">Error mesxsage.</param>
    /// <param name="terminateConnection">Whether to terminate the connection.</param>
    /// <param name="exitCode">Exit code.</param>
    protected async Task ErrorExit(bool sendErrorMessage, string? errorMessage, bool terminateConnection, int exitCode)
    {
        if(sendErrorMessage)
        {
            errorMessage ??= "unknown error";   // This should never happen
            await ServerCommunicator.SendMessage(new ErrMessage(Config.DisplayName!, errorMessage));
        }

        if(terminateConnection)
            await ServerCommunicator.SendMessage(new ByeMessage(Config.DisplayName!));

        GracefulTermination();

        Environment.Exit(exitCode);
    }
}