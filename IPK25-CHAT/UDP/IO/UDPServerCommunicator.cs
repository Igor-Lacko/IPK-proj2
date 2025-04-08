/* Contains a class for UDP communication with the server */

namespace IPK_25_CHAT.UDP.IO;

using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Message;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Class for UDP communication with the server.
/// </summary>
/// <param name="host">The host IP address.</param>
/// <param name="initialPort">The initial port number.</param>
public class UDPServerCommunicator(IPAddress host, ushort initialPort) : IServerCommunicator
{
    /// <summary>
    /// The host IP address.
    /// </summary>
    private readonly IPAddress Host = host;

    /// <summary>
    /// The current port number.
    /// </summary>
    private ushort Port = initialPort;

    /// <summary>
    /// Socket for communication.
    /// </summary>
    private Socket? UdpSocket = null;

    /// <summary>
    /// Current message ID, starting from 0.
    /// </summary>
    private short currentMessageId = 0;

    /// <summary>
    /// Initializes the socket for initial connection.
    /// </summary>
    public void Initialize() => UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    /// <summary>
    /// Closes the socket.
    /// </summary>
    public void Close()
    {
        UdpSocket!.Shutdown(SocketShutdown.Both);
        UdpSocket!.Close();
    }

    /// <summary>
    /// Sends the initial message to the server.
    /// Needs to be split from the other messages due to the dynamic port switching.
    /// </summary>
    /// <param name="message">Initial AUTH message to send.</param>
    public async Task SendInitialMessage(AuthMessage message)
    {
        // Initial endpoint
        IPEndPoint initial = new(Host, Port);

        // Send the message
        await UdpSocket!.SendToAsync(message.AsBytes(currentMessageId++), SocketFlags.None, initial);

        // Receive CONFIRM
        byte[] buffer = new byte[1500]; // Max MTU size
        int received = (await UdpSocket!.ReceiveFromAsync(buffer, initial)).ReceivedBytes;

        // None received? TODO: Find out how to handle this.
        if(received == 0)
        {
            Console.WriteLine("No data received from server.");
            Environment.Exit(1);
        }
    }
}