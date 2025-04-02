using System.Threading.Tasks;
using IPK_25_CHAT.Arguments;

namespace IPK_25_CHAT;

class Program
{
    public static void Main(string[] args)
    {
        CommandLineArguments arguments = CommandLineArgumentParser.ParseCLIArgs(args);
        Console.WriteLine(arguments);
    }
}