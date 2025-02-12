using System.Threading.Tasks;
using src.Common;
using src.TCP;
using src.UDP;

class Program
{
    static async Task Main(string[] args)
    {
        CommandLineArguments arguments = new(args);

        // Run the UDP or TCP client based on the arguments
        if(arguments.protocol == NetworkProtocol.TCP)
        {
            await new TCPClient(arguments).RunAsync();
        }
        else
        {
            new UDPClient(arguments).Run();
        }
    }

    public static void PrintErrorAndHelp(string message)
    {
        ErrorLogger.ErrorMessage(message);
        PrintHelp(1);
    }

    public static void PrintHelp(int exitCode = 0)
    {
        Console.WriteLine("Usage: dotnet run -t [tcp | udp] -s [hostname | ip] [options], where the optional options are:");
        Console.WriteLine("-p [port] - the port to connect to (default 4567)");
        Console.WriteLine("-d [timeout] - the timeout for the UDP connection (default 250)");
        Console.WriteLine("-r [retransmissions] - the number of retransmissions for the UDP connection (default 3)");
        Console.WriteLine("-h - print this help message and exit the program");
        Environment.Exit(exitCode);
    }
}