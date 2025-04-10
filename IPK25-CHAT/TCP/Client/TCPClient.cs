/* Contains the TCPClient class, which handles the TCP variant of this program. */

namespace IPK_25_CHAT.TCP.Client;

using System.Net;
using IPK_25_CHAT.Client;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.TCP.IO;


/// <summary>
/// TCPClient class.
/// Inherits from the Client class.
/// </summary>
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
        ServerCommunicator = CreateServerCommunicator();
        ServerCommunicator.MessageReceived += MessageStorage.OnMessageReceived;
    }

    /// <summary>
    /// Server communicator for the TCP client.
    /// </summary>
    /// <returns>Server communicator for the TCP client.</returns>
    protected override IServerCommunicator CreateServerCommunicator() => new TCPServerCommunicator(Host, Port);
}