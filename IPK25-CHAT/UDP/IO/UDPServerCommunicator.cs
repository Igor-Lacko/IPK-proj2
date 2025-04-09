/* Contains a class for UDP communication with the server */

namespace IPK_25_CHAT.UDP.IO;

using IPK_25_CHAT.Enum;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Message;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Class for UDP communication with the server.
/// </summary>
/// <param name="host">The host IP address.</param>
/// <param name="initialPort">The initial port number.</param>
/// <param name="timeout">Timeout for receiving CONFIRM messages.</param>
public class UDPServerCommunicator(IPAddress host, ushort initialPort, ushort timeout, ushort retransmissions) : IServerCommunicator
{
    /// <summary>
    /// The host IP address.
    /// </summary>
    private readonly IPAddress Host = host;

    /// <summary>0
    /// The current port number.
    /// </summary>
    private ushort Port = initialPort;

    /// <summary>
    /// The timeout for receiving CONFIRM messages.
    /// </summary>
    private readonly ushort Timeout = timeout;

    /// <summary>
    /// The number of retransmissions for the UDP client.
    /// </summary>
    private readonly ushort NumberOfRetransmissions = retransmissions;

    /// <summary>
    /// List of already seen message IDs.
    /// </summary>
    private readonly List<ushort> SeenMessageIDs = [];

    /// <summary>
    /// Dictionary mapping sent message ID's to their current states.
    /// </summary>
    private readonly Dictionary<ushort, MessageStateInformation> SentMessageInformation = [];

    /// <summary>
    /// Socket for communication.
    /// </summary>
    private Socket? UdpSocket = null;

    /// <summary>
    /// Semaphore for sending messages. Ensures that a sent message is confirmed before sending another.
    /// </summary>
    private readonly SemaphoreSlim SendGuardian = new(1, 1);

    /// <summary>
    /// Current message ID, starting from 0.
    /// </summary>
    private ushort currentMessageId = 0;

    /// <summary>
    /// The event thrown when a message is received from the server when receiving in a loop.
    /// </summary>
    public event Action<Message> MessageReceived = message => { };

    /// <summary>
    /// The event throuwn when a message confirmation timeouts.
    /// </summary>
    public event Action ConfirmTimeouted = () => { };

    /// <summary>
    /// Cancellation token source. Cancels the server communicator at the end of the program.
    /// </summary>
    public CancellationTokenSource ServerInputCancellationToken { get; } = new();

    /// <summary>
    /// Initializes the socket for initial connection.
    /// </summary>
    public void Initialize()
    {
        UdpSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        UdpSocket.Bind(new IPEndPoint (IPAddress.Any, 0));
    }

    /// <summary>
    /// Closes the socket.
    /// </summary>
    public void Close()
    {
        // Already closed
        if(ServerInputCancellationToken.IsCancellationRequested) return;

        // Close
        ServerInputCancellationToken.Cancel();
        UdpSocket!.Shutdown(SocketShutdown.Both);
        UdpSocket!.Close();
    }

    /// <summary>
    /// Runs the server communicator.
    /// </summary>
    public void Run() => Task.Run(async () =>
    {
        while(!ServerInputCancellationToken.IsCancellationRequested)
        {
            // Buffer to store the response, use the max MTU size
            byte[] buffer = new byte[1500];

            // Filter out depending on if the socket is connected or not
            if(!UdpSocket!.Connected)
            {
                // Endpoint to store
                IPEndPoint remoteEndPoint = new(IPAddress.Any, 0);

                // Receive the message
                await UdpSocket!.ReceiveFromAsync(buffer, remoteEndPoint);

                // Filter out non-server IP addresses at the beginning
                if(remoteEndPoint.Address != Host) continue;

                // Update the port if the server switched (on the initial AUTH) and connect to only receive from the server
                if(remoteEndPoint.Port != Port)
                {
                    // Reset port (todo remove)
                    Port = (ushort)remoteEndPoint.Port;

                    // Use connected UDP socket to only receive messages from the server
                    UdpSocket!.Connect(new IPEndPoint(Host, Port));
                }
            }

            // If the socket is connected, we can just use ReceiveAsync()
            else await UdpSocket!.ReceiveAsync(buffer);

            // Parse the mesasge and handle it
            await HandleReceivedMessage(Message.Parse(buffer));
        }
    });

    /// <summary>
    /// Handler method for received messages.
    /// Needed here as opposed to TCPServerCommunicator, since the latter just delegates all messages to the client.
    /// Here we need to handle CONFIRM and PING internally, while checking for already seen message IDs. 
    /// </summary>
    /// <param name="message">The message to handle.</param>
    public async Task HandleReceivedMessage(Message message)
    {
        // No checks needed here
        if(message.Type == MessageType.MALFORMED)
        {
            MessageReceived.Invoke(message);
            return;
        }

        // CONFIRM received --> Set the task result for the confirmed message
        else if(message.Type == MessageType.CONFIRM)
        {
            SentMessageInformation[message.GetMessageID()].OnConfirm.SetResult(true);
            SentMessageInformation.Remove(message.GetMessageID());
        }

        // Check if the ID was seen already, if yes send confirm and return
        else if(SeenMessageIDs.Contains(message.GetMessageID()))
        {
            await SendConfirm(message.GetMessageID());
            return;
        }

        // PING received --> Send CONFIRM
        else if(message.Type == MessageType.PING)
        {
            ushort messageID = message.GetMessageID();
            SeenMessageIDs.Add(messageID);
            await SendConfirm(messageID);
            return;
        }

        // Non PING/CONFIRM message which was also not seen before --> delegate to the client class
        else
        {
            SeenMessageIDs.Add(message.GetMessageID());
            MessageReceived.Invoke(message);
        }
    }

    /// <summary>
    /// Sends a CONFIRM message to the server.
    /// Kept separate from the SendMessage method, since we don't need to wait for CONFIRM to be, well, confirmed.
    /// </summary>
    /// <param name="messageID">ID of the message to confirm.</param>
    public async Task SendConfirm(ushort messageID)
    {
        await SendGuardian.WaitAsync();

        // Send confirm
        if(UdpSocket!.Connected) await UdpSocket.SendAsync(new ConfirmMessage(messageID).AsBytes(messageID));
        else await UdpSocket!.SendToAsync(new ConfirmMessage(messageID).AsBytes(messageID), new IPEndPoint(Host, Port));

        // Release the semaphore
        SendGuardian.Release();
    }

    /// <summary>
    /// Sends a message to the server.
    /// Needs to be split from the other messages due to the dynamic port switching.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <returns>True if the message was confirmed, false otherwise.</returns>
    public async Task SendMessage(Message message)
    {
        // Wait for access to sending
        await SendGuardian.WaitAsync();

        // Current message ID
        ushort messageID = currentMessageId++;

        // Create a object for the state of the message
        MessageStateInformation messageState = new(messageID, new TaskCompletionSource<bool>());
        SentMessageInformation[messageID] = messageState;

        // Current destination
        IPEndPoint current = new(Host, Port);

        // Send the message
        byte[] messageAsBytes = message.AsBytes(messageID);

        // Depending on if we were connected or not
        if(UdpSocket!.Connected) await UdpSocket.SendAsync(messageAsBytes);
        else await UdpSocket!.SendToAsync(messageAsBytes, current);

        // Flag
        bool confirmed = false;

        // Wait for confirmation for RETRANSMISSIONS number of times
        for(int i = 0; i < NumberOfRetransmissions; i++)
        {
            Task<bool> completedTask = messageState.MessageConfirmed;
            Task timeoutTask = Task.Delay(Timeout);
            Task completed = await Task.WhenAny(completedTask, timeoutTask);

            // Message was confirmed
            if(completed == completedTask)
            {
                confirmed = true;
                break;
            }

            // Again, send depending on the state of the socket connection
            else
            {
                // Send the message again
                if(UdpSocket!.Connected) await UdpSocket.SendAsync(messageAsBytes);
                else await UdpSocket!.SendToAsync(messageAsBytes, current);
            }
        }

        // Invoke the timeout event if the message was not confirmed and close the connection
        if(!confirmed)
        {
            UdpSocket!.Close();
            ConfirmTimeouted.Invoke();
        }

        // Else release the semaphore
        else SendGuardian.Release();
    }
}