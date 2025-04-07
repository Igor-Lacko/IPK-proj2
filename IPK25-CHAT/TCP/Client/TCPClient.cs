/* Contains the TCPClient class, which handles the TCP variant of this program. */

namespace IPK_25_CHAT.TCP.Client;

using System.Net;
using IPK_25_CHAT.Client;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Command;
using IPK_25_CHAT.Enum;
using IPK_25_CHAT.IO;
using IPK_25_CHAT.TCP.IO;
using IPK_25_CHAT.Message;
using System.Threading.Tasks;

/// <summary>
/// TCPClient class.
/// Inherits from the Client class.
/// </summary>
/// 
public class TCPClient : Client
{
    /// <summary>
    /// Constructor for the TCPClient class.
    /// </summary>
    /// <param name="host">IP address of the server.</param>
    /// <param name="port">Port number of the server.</param>
    public TCPClient(IPAddress host, ushort port) : base(host, port)
    {
        Host = host;
        Port = port;
    }

    /// <summary>
    /// Server communicator for the TCP client.
    /// </summary>
    /// <returns>Server communicator for the TCP client.</returns>
    protected override IServerCommunicator CreateServerCommunicator() => new TCPServerCommunicator(Host, Port);

    /// <summary>
    /// Gracefully closes the client.
    /// <param name="sendBye">Whether to send a BYE message to the server.</param>
    /// </summary>
    protected override void GracefulTermination()
    {
        // Close the server communicator
        ServerCommunicator.Close();

        // Close the UserInputReader
        InputReader.Close();
    }

    /// <summary>
    /// Executes the AUTH command.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    protected override async Task ExecuteAuthCommand(AuthCommand command)
    {
        // Set user parameters
        UpdateUsername(command.Username);
        UpdateDisplayName(command.DisplayName);

        // Open connection if not done already
        if(ClientState == State.START)
        {
            ServerCommunicator.Initialize();
            ServerCommunicator.Run();
        }

        // Send the AUTH message to the server
        await ServerCommunicator.SendMessage(new AuthMessage(command));
        UpdateState(State.AUTH);
    }

    /// <summary>
    /// Executes the JOIN command.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    protected override async Task ExecuteJoinCommand(JoinCommand command)
    {
        await ServerCommunicator.SendMessage(new JoinMessage(Config.DisplayName!, command.ChannelId));
        UpdateRequestedChannelID(command.ChannelId);
        UpdateState(State.JOIN);
    }

    /// <summary>
    /// Called upon receiving server input. Checks if the mesage is not valid for the current state.
    /// Also checks for ERR/BYE and MALFORMED messages.
    /// </summary>
    /// <param name="message">The message received.</param>
    protected override async Task<bool> TerminatingMessageReceived(Message message)
    {
        // Invalid message for the given state
        if(!message.IsValid(ClientState))
            await ErrorExit(true, $"Invalid message type {message.Type} in state {ClientState}", true);

        // Maybe ERR/BYE/MALFORMED
        switch(message.Type)
        {
            // Print the message and go to END
            case MessageType.ERR:
                StdoutResultWriter.PrintErrMessage((ErrMessage)message);
                await ErrorExit(false, null, true);
                return true;

            // Go to END
            case MessageType.BYE:
                UpdateState(State.END);
                return true;

            // Print a local error and terminate
            case MessageType.MALFORMED:
                StdoutResultWriter.InternalClientError(((MalformedMessage)message).MessageContent);
                await ErrorExit(true, "Malformed message received", true);
                return true;
        }

        // Valid message
        return false;
    }

    /// <summary>
    /// Handles the AUTH state. Waits for a reply from the server.
    /// </summary>
    protected override async Task AuthState()
    {
        // Wait for a message from the server or a timeout
        Task TimeoutTask = Task.Delay(5000);
        Task<Message> ServerInputTask = MessageStorage.WaitForInput(CancellationToken.None);
        Task completedTask = await Task.WhenAny(ServerInputTask, TimeoutTask);

        // Check if the timeout task completed first
        if(completedTask == TimeoutTask)
        {
            StdoutResultWriter.InternalClientError("Timeout when waiting for reply to authentication");
            await ErrorExit(true, "Timeout when waiting for reply to authentication", true);
            return;
        }

        Message reply = ServerInputTask.Result;

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
        Task<Message> serverInputTask = MessageStorage.WaitForInput(readInputCancel.Token);

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
                await ErrorExit(true, "Reply message received when not waiting for it!", true);
        }

    }

    /// <summary>
    /// Handles the OPEN state. This is the state where we are connected into a channel and print/send messages.
    /// </summary>
    protected override async Task OpenState()
    {
        // Loop while we are receivinf messages
        while(ClientState == State.OPEN)
        {
            // To cancel the non-finished task
            using var cts = new CancellationTokenSource();

            // Wait for a message from the server or user input
            Task<Message> serverInputTask = MessageStorage.WaitForInput(cts.Token);
            Task<string?> userInputTask = UserInputQueue.Dequeue(cts.Token);

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
                if(userInputTask.Result == null) OnEofReceived();

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
    /// Handles the JOIN state. Waits for a reply from the server, while printing any incoming messages.
    /// </summary>
    protected override async Task JoinState()
    {
        // Until something triggers a state change
        while(ClientState == State.JOIN)
        {
            // Wait for server input or a timeout
            Task TimeoutTask = Task.Delay(5000);
            Task<Message> ServerInputTask = MessageStorage.WaitForInput(CancellationToken.None);
            Task completedTask = await Task.WhenAny(ServerInputTask, TimeoutTask);

            // Check if the timeout task completed first
            if(completedTask == TimeoutTask)
            {
                StdoutResultWriter.InternalClientError("Timeout when waiting for reply to join a chat channel");
                await ErrorExit(true, "Timeout when waiting for reply to join a chat channel", true);
                return;
            }

            Message message = ServerInputTask.Result;

            // Check if the message is a terminating message
            if(await TerminatingMessageReceived(message))
                return;

            // Reply or a normal message

            // Normal message
            else if(message.Type == MessageType.MSG)
            {
                // Print the message
                StdoutResultWriter.PrintMsgMessage((MsgMessage)message);
                continue;
            }

            // Reply from the server
            else
            {
                ReplyMessage reply = (ReplyMessage)message;
                StdoutResultWriter.PrintReplyMessage(reply);

                // Change chat channel if the reply is positive
                if(reply.OK) UpdateChannelID(Config.RequestedChannelID!);

                // Set the requested ID to null, change the client state to OPEN and return
                UpdateRequestedChannelID(null);
                UpdateState(State.OPEN);
                return;
            }
        }
    }

    /// <summary>
    /// Handles the END state.
    /// </summary>
    protected override void EndState()
    {
        GracefulTermination();
        Environment.Exit(0);
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
    protected override async Task ErrorExit(bool sendErrorMessage = false, string? errorMessage = null, bool terminateConnection = false, int exitCode = 1)
    {
        if(sendErrorMessage)
        {
            if(errorMessage == null) throw new ArgumentException("prosim igino oprav si kod");
            await ServerCommunicator.SendMessage(new ErrMessage(Config.DisplayName!, errorMessage));
        }

        if(terminateConnection)
            await ServerCommunicator.SendMessage(new ByeMessage(Config.DisplayName!));

        GracefulTermination();

        Environment.Exit(exitCode);
    }
}