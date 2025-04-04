/* Implementation of a structure representing one instance of CLI arguments. */

namespace IPK_25_CHAT.Arguments;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// Structure representing an instance of CLI arguments.
/// </summary>
public struct CommandLineArguments
{
    /// <summary>
    /// TCP/UDP protocol type.
    /// </summary>
    public ProtocolType? Protocol;

    /// <summary>
    /// Server IP address.
    /// </summary>
    public IPAddress? Address;

    /// <summary>
    /// Server port.
    /// </summary>
    public ushort? Port;

    /// <summary>
    /// UDP confirmation timeout in milliseconds.
    /// </summary>
    public ushort? Timeout;

    /// <summary>
    /// Max number of UDP retransmissions.
    /// </summary>
    public byte? Retransmissions;

    /// <summary>
    /// If using the extended notation for channel ids, e.g. discord.CHANNLE_ID
    /// </summary>
    public bool Discord;

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
        Discord = false;
    }
}