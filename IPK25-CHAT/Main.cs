using IPK_25_CHAT.Arguments;
using IPK_25_CHAT.TCP.Client;
using IPK_25_CHAT.UDP.Client;
using System.Net.Sockets;

namespace IPK_25_CHAT;

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