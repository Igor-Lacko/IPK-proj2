/* Contains the TCPClient class, which handles the TCP variant of this program. */

namespace IPK_25_CHAT.TCP.Client;

using System.Net;
using System.Net.Sockets;
using IPK_25_CHAT.Client;
using IPK_25_CHAT.Command;
using IPK_25_CHAT.Enum;
using IPK_25_CHAT.IO;
using IPK_25_CHAT.Message;

/// <summary>
/// TCPClient class.
/// Inherits from the Client class.
/// </summary>
/// <param name="host">IP address of the host.</param>
/// <param name="port">Port of the host.</param>
public class TCPClient(IPAddress host, ushort port) : Client(host, port)
{
    
}