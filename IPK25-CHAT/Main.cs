/* IPK, project 2: Client for a chat server
    - Author: Igor Lacko (xlackoi00)
*/

namespace IPK_25_CHAT;

using IPK_25_CHAT.TCP;
using IPK_25_CHAT.UDP;
using IPK_25_CHAT.Arguments;

using System.Net.Sockets;

/// <summary>
/// Entrance class of the client.
/// </summary>
class Program
{
    public static void Main(string[] args)
    {
        CommandLineArguments arguments = CommandLineArgumentParser.ParseCLIArgs(args);

        // Run eiter TCP or UDP client
        if(arguments.Protocol == ProtocolType.Tcp)
        {
            TCPClient client = new(arguments.Address!, (ushort)arguments.Port!);
            client.Run().Wait();
        }

        else
        {
            UDPClient client = new(arguments.Address!, (ushort)arguments.Port!, (ushort)arguments.Retransmissions!, (ushort)arguments.Timeout!);
            client.Run().Wait();
        }
    }
}