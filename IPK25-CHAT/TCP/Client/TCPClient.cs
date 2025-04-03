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
    protected override void ExecuteAuthCommand(AuthCommand command)
    {
        // Set user parameters
        Config.Username = command.Username;
        Config.DisplayName = command.DisplayName;

        // Send the AUTH message to the server
        ServerCommunicator.SendMessage(new AuthMessage(command));
        ClientState = State.AUTH;
    }

    /// <summary>
    /// Executes the JOIN command.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    protected override void ExecuteJoinCommand(JoinCommand command) => ServerCommunicator.SendMessage(new JoinMessage(Config.DisplayName!, command.ChannelId));

    /// <summary>
    /// Handles the AUTH state. Waits for a reply from the server.
    /// </summary>
    protected override async Task AuthState()
    {

        // Cancellation token source to only wait for one task
        using var inputReadCancel = new CancellationTokenSource();

        // Tasks for user/server input
        Task<string?> userInputTask = UserInputQueue.Dequeue(inputReadCancel.Token);
        Task<Message> serverInputTask = ServerInputQueue.Dequeue(inputReadCancel.Token);

        // Wait for either task to finish
        Task finishedTask = await Task.WhenAny(userInputTask, serverInputTask);

        // Cancel the other task
        inputReadCancel.Cancel();

        // If the user input task completed first, validate the input
        if(finishedTask == userInputTask)
        {
            // Ctrl+C or Ctrl+D
            if(userInputTask.Result == null) OnEofReceived();

            // Validate the input
            IReadable? input = InputValidator.Validate(userInputTask.Result!);
            if(input == null) return;

            // Execute potential commands, basically only help or rename
            RunUserInput(input);
        }

        // Got a response from the server
        else if(finishedTask == serverInputTask)
        {
            Message message = await serverInputTask;

            // Malformed or invalid message
            if(message.Type == MessageType.MALFORMED)
            {
                // Malformed message, print it and exit
                StdoutResultWriter.InternalClientError(((MalformedMessage)message).MessageContent);
                ErrorExit(true, "Malformed message received!", true);
            }

            // Invalid message for the given state, if it's MSG print it and exit the program
            else if(!message.IsValid(ClientState))
                ErrorExit(true, "Invalid message for AUTH state!", true);

            // Valid message
            else switch(message.Type)
            {
                // Either stay in AUTH or go to OPEN
                case MessageType.REPLY:
                    StdoutResultWriter.PrintReplyMessage((ReplyMessage)message);
                    ClientState = ((ReplyMessage)message).OK ? State.OPEN : State.AUTH;
                    return;

                case MessageType.ERR:
                    StdoutResultWriter.PrintErrMessage((ErrMessage)message);
                    ServerCommunicator.SendMessage(new ByeMessage(Config.DisplayName!));
                    ClientState = State.END;
                    return;

                case MessageType.BYE:
                    ServerCommunicator.SendMessage(new ByeMessage(Config.DisplayName!));
                    ClientState = State.END;
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

    protected override Task JoinState()
    {
        throw new NotImplementedException();
    }

    protected override Task OpenState()
    {
        throw new NotImplementedException();
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
    protected override void ErrorExit(bool sendErrorMessage = false, string? errorMessage = null, bool terminateConnection = false, int exitCode = 1)
    {
        if(sendErrorMessage)
        {
            if(errorMessage == null) throw new ArgumentException("prosim igino oprav si kod");
            ServerCommunicator.SendMessage(new ErrMessage(Config.DisplayName!, errorMessage));
        }

        if(terminateConnection)
            ServerCommunicator.SendMessage(new ByeMessage(Config.DisplayName!));

        GracefulTermination();

        Environment.Exit(exitCode);
    }
}