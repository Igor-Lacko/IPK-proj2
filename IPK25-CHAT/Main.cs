using System.Threading.Tasks;
using IPK_25_CHAT.Arguments;
using IPK_25_CHAT.TCP.Client;

namespace IPK_25_CHAT;

class Program
{
    public static void Main(string[] args)
    {
        CommandLineArguments arguments = CommandLineArgumentParser.ParseCLIArgs(args);
        TCPClient client = new(arguments.Address!, (ushort)arguments.Port!);
        client.Run().Wait();
    }
}