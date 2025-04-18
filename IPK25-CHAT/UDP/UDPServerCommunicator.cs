/* Contains a class for UDP communication with the server */

namespace IPK_25_CHAT.UDP;

using IPK_25_CHAT.Message;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Enum;
using System.Net;
using System.Net.Sockets;
using IPK_25_CHAT.IO;

/// <summary>
/// Class for UDP communication with the server.
/// </summary>
public class UDPServerCommunicator : IServerCommunicator
{
    /// <summary>
    /// The host IP address.
    /// </summary>
    private readonly IPAddress Host;

    /// <summary>0
    /// The current port number.
    /// </summary>
    private ushort Port;

    /// <summary>
    /// The timeout for receiving CONFIRM messages.
    /// </summary>
    private readonly ushort Timeout;

    /// <summary>
    /// The number of retransmissions for the UDP client.
    /// </summary>
    private readonly ushort NumberOfRetransmissions;

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
    private readonly Socket UdpSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    /// <summary>
    /// Semaphore for sending messages. Ensures that a sent message is confirmed before sending another.
    /// </summary>
    private readonly SemaphoreSlim SendGuardian = new(1, 1);

    /// <summary>
    /// Current message ID, starting from 0.
    /// </summary>
    private ushort CurrentMessageID = 0;

    /// <summary>
    /// The event thrown when a message is received from the server when receiving in a loop.
    /// </summary>
    public event Action<Message> MessageReceived = message => { };

    /// <summary>
    /// The event thrown when a message confirmation timeouts.
    /// </summary>
    public event Action ConfirmTimeouted = () => { };

    /// <summary>
    /// Cancellation token source. Cancels the server communicator at the end of the program.
    /// </summary>
    public CancellationTokenSource ServerInputCancellationTokenSource { get; } = new();

    /// <summary>
    /// Constructor for the UDPServerCommunicator class.
    /// </summary>
    /// <param name="host">The host IP address.</param>
    /// <param name="initialPort">The initial port number.</param>
    /// <param name="timeout">Timeout for receiving CONFIRM messages.</param>
    /// <param name="retransmissions">Number of retries for one message until the communicator gets a confirmation.</param>
    public UDPServerCommunicator(IPAddress host, ushort initialPort, ushort timeout, ushort retransmissions)
    {
        Host = host;
        Port = initialPort;
        Timeout = timeout;
        NumberOfRetransmissions = retransmissions;

        // Bind the socket
        UdpSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
    }

    /// <summary>
    /// Closes the socket.
    /// </summary>
    public void Close()
    {
        try
        {
            ServerInputCancellationTokenSource.Cancel();
            UdpSocket.Shutdown(SocketShutdown.Both);
            UdpSocket.Close();
        }

        catch
        {
            return;
        }
    }

    /// <summary>
    /// Runs the server communicator.
    /// </summary>
    public void Run() => Task.Run(async () =>
    {
        while(!ServerInputCancellationTokenSource.IsCancellationRequested)
        {
            // Buffer to store the response, use the max MTU size
            byte[] buffer = new byte[1500];

            // Bytes received
            int bytesReceived = 0;

            // Depending on if the socket is connected or not
            if(!UdpSocket.Connected)
            {
                // Receive the message
                SocketReceiveFromResult result = await UdpSocket.ReceiveFromAsync(buffer, new IPEndPoint(IPAddress.Any, 0));
                bytesReceived = result.ReceivedBytes;
                IPEndPoint remoteEndPoint = (IPEndPoint)result.RemoteEndPoint;

                // Filter out non-server IP addresses at the beginning
                if(!remoteEndPoint.Address.Equals(Host))
                    continue;

                // Update the port if the server switched (on the initial AUTH) and connect to only receive from the server
                if(remoteEndPoint.Port != Port)
                {
                    // Reset port
                    Port = (ushort)remoteEndPoint.Port;

                    // Connected UDP socket to only receive messages from the server
                    try
                    {
                        UdpSocket.Connect(new IPEndPoint(Host, Port));
                    }

                    catch(SocketException e)
                    {
                        StdoutResultWriter.InternalClientError($"Failed to connect: {e.Message}");
                        Environment.Exit((int)ExitCodes.ERROR_OTHER);
                    }
                }
            }

            // If the socket is connected, we can just use ReceiveAsync()
            else bytesReceived = await UdpSocket.ReceiveAsync(buffer);

            // Parse the message and handle it
            await HandleReceivedMessage(Message.Parse(buffer, bytesReceived));
        }
    });

    /// <summary>
    /// Handler method for received messages.
    /// Needed here as opposed to TCPServerCommunicator, since the latter just delegates all messages to the client.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    private async Task HandleReceivedMessage(Message message)
    {
        // Check if we got an ID, if yes send a confirm if not just pass the malformed message to the client
        if(message.Type == MessageType.MALFORMED)
        {
            MalformedMessage malformed = (MalformedMessage)message;
            if(malformed.MessageID != null) await SendConfirm((ushort)malformed.MessageID!);
            MessageReceived.Invoke(message);
        }

        // CONFIRM received --> Set the task result for the confirmed message (if it exists)
        else if(message.Type == MessageType.CONFIRM)
        {
            if(SentMessageInformation.TryGetValue(message.GetMessageID(), out MessageStateInformation value))
            {
                value.OnConfirm.SetResult(true);
                if(!value.IsRequest) SentMessageInformation.Remove(message.GetMessageID());
            }

            // Treat as malformed
            else
            {
                MalformedMessage malformed = new(null);
                MessageReceived.Invoke(malformed);
            }
        }

        // Check if the ID was seen already, if yes send confirm and return
        else if(SeenMessageIDs.Contains(message.GetMessageID()))
            await SendConfirm(message.GetMessageID());

        // Same for REPLY messages that are replying to a non-request message or a unknown message
        else if(message.Type == MessageType.REPLY)
        {
            // Mark the message as being already seen, extract the ref id and send confirm
            ushort messageID = message.GetMessageID();
            ushort refMessageID = ((ReplyMessage)message).RefMessageID;
            SeenMessageIDs.Add(messageID);
            await SendConfirm(messageID);

            // Verify if it's replying to a request
            if(SentMessageInformation.TryGetValue(refMessageID, out MessageStateInformation maybeRequest) && maybeRequest.IsRequest && maybeRequest.MessageConfirmed.IsCompleted)
            {
                // We shouldn't need a lock here since there can only be one request message at a time
                SentMessageInformation.Remove(refMessageID);
                MessageReceived.Invoke(message);
            }

            // Treat as malformed message, and send a confirm since replies have valid ID's
            else
            {
                await SendConfirm(messageID);
                MalformedMessage malformed = new(null);
                MessageReceived.Invoke(malformed);
            }
        }

        // PING received --> Send CONFIRM
        else if(message.Type == MessageType.PING)
        {
            ushort messageID = message.GetMessageID();
            SeenMessageIDs.Add(messageID);
            await SendConfirm(messageID);
        }

        // Non PING/CONFIRM message which was also not seen before --> delegate to the client class
        else
        {
            ushort messageID = message.GetMessageID();
            SeenMessageIDs.Add(messageID);
            await SendConfirm(messageID);
            MessageReceived.Invoke(message);
        }
    }

    /// <summary>
    /// Sends a CONFIRM message to the server.
    /// Kept separate from the SendMessage method, since we don't need to wait for CONFIRM to be, well, confirmed.
    /// Also the semaphore is not neccessary here, since the risk of spamming the server is not present here (because
    /// CONFIRM is only sent as a response to the server and it caused issues when sending 
    /// confirm waited on a semaphore (for example when piping input from a file).
    /// </summary>
    /// <param name="messageID">ID of the message to confirm.</param>
    private async Task SendConfirm(ushort messageID)
    {
        if(UdpSocket!.Connected) await UdpSocket.SendAsync(new ConfirmMessage(messageID).AsBytes(messageID));
        else await UdpSocket!.SendToAsync(new ConfirmMessage(messageID).AsBytes(messageID), new IPEndPoint(Host, Port));
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public async Task SendMessage(Message message)
    {
        try
        {
            // Until the previous message is not confirmed
            await SendGuardian.WaitAsync(ServerInputCancellationTokenSource.Token);
        }

        catch(OperationCanceledException)
        {
            return;
        }

        // Current message ID
        ushort messageID = CurrentMessageID++;

        // Create a object for the state of the message
        MessageStateInformation messageState = new(messageID, new TaskCompletionSource<bool>(), message.Type == MessageType.AUTH || message.Type == MessageType.JOIN);
        SentMessageInformation[messageID] = messageState;

        // Current destination
        IPEndPoint current = new(Host, Port);

        // Send the message
        byte[] messageAsBytes = message.AsBytes(messageID);

        // Depending on if we were connected or not
        if(UdpSocket.Connected) await UdpSocket.SendAsync(messageAsBytes);
        else await UdpSocket.SendToAsync(messageAsBytes, current);

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
                if(UdpSocket.Connected) await UdpSocket.SendAsync(messageAsBytes);
                else await UdpSocket.SendToAsync(messageAsBytes, current);
            }
        }

        // Invoke the timeout event if the message was not confirmed and close the connection
        if(!confirmed)
        {
            UdpSocket.Close();
            ConfirmTimeouted.Invoke();
        }

        else SendGuardian.Release();
    }
}