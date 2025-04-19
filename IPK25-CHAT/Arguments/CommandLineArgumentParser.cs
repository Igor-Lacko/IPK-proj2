namespace IPK_25_CHAT.Arguments;

using IPK_25_CHAT.Command;
using IPK_25_CHAT.Enum;

using System.Net;
using System.Net.Sockets;

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
        Console.WriteLine("  -d [TIMEOUT]       UDP confirmation timeout in milliseconds. Default is 250");
        Console.WriteLine("  -r [RETRANSMISSIONS] Max number of UDP retransmissions. Default is 3");
        Console.WriteLine("  --discord         Extra argument. Enables Discord notation for channel id's, is turned off by default.");
        Console.WriteLine("  --help             Prints this help and exits");
        Environment.Exit((int)ExitCodes.SUCCESS);
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
                    if(protocol == "tcp") arguments.Protocol = ProtocolType.Tcp;

                    // UDP
                    else if(protocol == "udp") arguments.Protocol = ProtocolType.Udp;

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
                        foreach(IPAddress ipAddress in Dns.GetHostAddresses(address))
                        {
                            // Check if the address is IPv4
                            if(ipAddress.AddressFamily == AddressFamily.InterNetwork)
                            {
                                arguments.Address = ipAddress;
                                break;
                            }
                        }
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
                case "-d":
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

                    if(!ushort.TryParse(args[++i], out ushort retransmissions))
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

                // Discord command
                case "--discord":
                    // Already defined case
                    if(arguments.Discord)
                    {
                        Console.Error.WriteLine("ERROR: Discord already set!");
                        Environment.Exit(1);
                    }

                    arguments.Discord = true;

                    break;
            }
        }

        // Check needed values
        CheckNeededValues(arguments);

        // Set default values
        arguments = SetDefaultValues(arguments);

        // Set the regex for join if the discord argument is set
        if(arguments.Discord) JoinCommand.ToggleDiscordNotation();

        return arguments;
    }
}