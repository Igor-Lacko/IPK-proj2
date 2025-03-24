/* Implementation of a structure representing one instance of CLI arguments. */

namespace src.Arguments;

using System.Net;
using System.Net.Sockets;

public struct CommandLineArguments
{
    public ProtocolType? Protocol;              // TCP/UDP
    public IPAddress? Address;                  // Server IP address
    public ushort? Port;                        // Server port
    public ushort? Timeout;                     // UDP confirmation timeout in milliseconds
    public byte? Retransmissions;               // Max number of UDP retransmissions

    /// <summary>
    /// Constructor. Sets all fields to null.
    /// </summary>
    public CommandLineArguments()
    {
        Protocol = null;
        Address = null;
        Port = null;
        Timeout = null;
        Retransmissions = null;
    }
}