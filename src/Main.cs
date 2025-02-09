using src.Common;
using src.TCP;
using src.UDP;

class Program
{
    public enum NetworkProtocol
    {
        TCP,
        UDP
    }

    static void Main(string[] args)
    {
        CommandLineArguments arguments = new(args);

        // Run the UDP or TCP client based on the arguments
        if(arguments.protocol == NetworkProtocol.TCP)
        {
            new TCPClient(arguments).Run();
        }
        else
        {
            new UDPClient(arguments).Run();
        }
    }

    public static void PrintHelp()
    {
        Console.WriteLine("Usage: dotnet run -t [tcp | udp] -s [hostname | ip] [options], where the optional options are:");
        Console.WriteLine("-p [port] - the port to connect to (default 4567)");
        Console.WriteLine("-d [timeout] - the timeout for the UDP connection (default 250)");
        Console.WriteLine("-r [retransmissions] - the number of retransmissions for the UDP connection (default 3)");
        Console.WriteLine("-h - print this help message and exit the program");
        Environment.Exit(0);
    }
}