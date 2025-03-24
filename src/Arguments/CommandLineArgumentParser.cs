/* Implementation of argument parsing and filling the CommandLineArguments struct */

using System.Net;
using System.Net.Sockets;

namespace src.Arguments;

/// <summary>
/// Contains methods for parsing and filling the CommandLineArguments struct.
/// </summary>
public static class CommandLineArgumentParser
{
    /// <summary>
    /// Prints the guide to the program and exits with code 0.
    /// </summary>
    public static void PrintHelp()
    {
        Console.WriteLine("Usage: ./ipk25chat-client -t [PROTOCOL] -s [ADDRESS|HOSTNAME] OPTIONS, where OPTIONS are:");
        Console.WriteLine("  -p [PORT]          Server port number. Default is 4567");
        Console.WriteLine("  -w [TIMEOUT]       UDP confirmation timeout in milliseconds. Default is 250");
        Console.WriteLine("  -r [RETRANSMISSIONS] Max number of UDP retransmissions. Default is 3");
        Console.WriteLine("  --help             Prints this help and exits");
        Environment.Exit(0);
    }

    /// <summary>
    /// Sets default values for the CommandLineArguments struct, if they weren't set already.
    /// </summary>
    /// <param name="arguments">CommandLineArguments instance.</param>
    /// <returns>CommandLineArguments with default values set if needed.</returns>
    public static CommandLineArguments SetDefaultValues(CommandLineArguments arguments)
    {
        arguments.Port ??= 4567;
        arguments.Timeout ??= 250;
        arguments.Retransmissions ??= 3;

        return arguments;
    }

    /// <summary>
    /// Checks if all needed values are set in the CommandLineArguments struct.
    /// </summary>
    /// <param name="arguments">CommandLineArguments instance.</param>
    public static void CheckNeededValues(CommandLineArguments arguments)
    {
        if(arguments.Protocol == null)
        {
            Console.Error.WriteLine("ERROR: Protocol not set!");
            Environment.Exit(1);
        }

        if(arguments.Address == null)
        {
            Console.Error.WriteLine("ERROR: Address not set!");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Parses CLI arguments and fills the CommandLineArguments struct.
    /// </summary>
    /// <param name="args">CLI argument instance.</param>
    /// <returns>A dilled CommandLineArguments structure.</returns>
    public static CommandLineArguments ParseCLIArgs(string[] args)
    {
        CommandLineArguments arguments = new();

        // Loop through all arguments
        for(int i = 0; i < args.Length; i++)
        {
            switch(args[i])
            {
                // Protocol
                case "-t":
                    // Already defined case
                    if(arguments.Protocol != null)
                    {
                        Console.Error.WriteLine("ERROR: Protocol already set!");
                        Environment.Exit(1);
                    }

                    // Get the next argument
                    string protocol = args[++i];

                    // TCP
                    if(protocol == "TCP") arguments.Protocol = ProtocolType.Tcp;

                    // UDP
                    else if(protocol == "UDP") arguments.Protocol = ProtocolType.Udp;

                    // Invalid protocol
                    else
                    {
                        Console.Error.WriteLine("ERROR: Invalid protocol type! See ./ipk25chat-client --help for more information.");
                        Environment.Exit(1);
                    }

                    break;

                // Address/Hostname
                case "-s":
                    // Already defined case
                    if(arguments.Address != null)
                    {
                        Console.Error.WriteLine("ERROR: Address already set!");
                        Environment.Exit(1);
                    }

                    // Get the next argument
                    string address = args[++i];

                    // Try to resolve as IPAddress first
                    if(IPAddress.TryParse(address, out IPAddress? ip))
                    {
                        arguments.Address = ip;
                        break;
                    }

                    // Try to resolve thorugh DNS
                    try
                    {
                        arguments.Address = Dns.GetHostAddresses(address)[0];
                    }

                    // Invalid address
                    catch
                    {
                        Console.Error.WriteLine($"ERROR: Invalid address/hostname {address}!");
                        Environment.Exit(1);
                    }

                    break;

                // Port
                case "-p":
                    // Already defined case
                    if(arguments.Port != null)
                    {
                        Console.Error.WriteLine("ERROR: Port already set!");
                        Environment.Exit(1);
                    }

                    if(!ushort.TryParse(args[++i], out ushort port))
                    {
                        Console.Error.WriteLine($"ERROR: Invalid port number {port}!");
                        Environment.Exit(1);
                    }

                    arguments.Port = port;
                    break;

                // Timeout
                case "-w":
                    // Already defined case
                    if(arguments.Timeout != null)
                    {
                        Console.Error.WriteLine("ERROR: Timeout already set!");
                        Environment.Exit(1);
                    }

                    if(!ushort.TryParse(args[++i], out ushort timeout))
                    {
                        Console.Error.WriteLine($"ERROR: Invalid timeout value {timeout}!");
                        Environment.Exit(1);
                    }

                    arguments.Timeout = timeout;
                    break;

                // Retransmissions
                case "-r":
                    // Already defined case
                    if(arguments.Retransmissions != null)
                    {
                        Console.Error.WriteLine("ERROR: Retransmissions already set!");
                        Environment.Exit(1);
                    }

                    if(!byte.TryParse(args[++i], out byte retransmissions))
                    {
                        Console.Error.WriteLine($"ERROR: Invalid retransmissions value {retransmissions}!");
                        Environment.Exit(1);
                    }

                    arguments.Retransmissions = retransmissions;
                    break;

                // Help command
                case "-h":
                    PrintHelp();
                    break;

                // Invalid argument
                default:
                    Console.Error.WriteLine($"ERROR: Invalid argument {args[i]}! See ./ipk25chat-client --help for more information.");
                    Environment.Exit(1);
                    break;
            }

        }

        // Check needed values
        CheckNeededValues(arguments);

        // Set default values
        arguments = SetDefaultValues(arguments);

        return arguments;
    }
}