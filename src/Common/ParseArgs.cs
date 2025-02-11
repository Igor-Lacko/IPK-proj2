/* This file contains the implementation of parsing CLI arguments. */

namespace src.Common;

class CommandLineArguments
{
    public NetworkProtocol protocol = NetworkProtocol.NONE;
    public string? hostname = null;
    public ushort port = 4567;
    public bool has_port = false;
    public ushort timeout = 250; // UDP only
    public bool has_timeout = false;
    public byte retransmissions = 3; // UDP only
    public bool has_retransmissions = false;

    public CommandLineArguments(string[] args)
    {
        ParseArgs(args);
    }

    /// <summary>
    /// Main CLI parsing logic. It assumes that the arguments are correctly provided, as per the assignment. TODO: Remove error handling if not allowed ig?
    /// </summary>
    public void ParseArgs(string[] args)
    {
        for(int i = 0; i < args.Length; i++)
        {
            switch(args[i])
            {
                case "-t":
                    if(protocol != NetworkProtocol.NONE) Program.PrintErrorAndHelp("Protocol already set");
                    if(i == args.Length - 1) Program.PrintErrorAndHelp("Missing argument for protocol");
                    protocol = args[++i] switch
                    {
                        "tcp" => NetworkProtocol.TCP,
                        "udp" => NetworkProtocol.UDP,
                        _ => NetworkProtocol.INVALID
                    };
                    if(protocol == NetworkProtocol.INVALID) Program.PrintErrorAndHelp($"Invalid protocol: {args[i]}");
                    break;

                case "-s":
                    if(hostname != null) Program.PrintErrorAndHelp("Hostname already set");
                    if(i == args.Length - 1) Program.PrintErrorAndHelp("Missing argument for hostname");
                    hostname = args[++i];
                    break;

                case "-p":
                    if(has_port) Program.PrintErrorAndHelp("Port already set");
                    if(i == args.Length - 1) Program.PrintErrorAndHelp("Missing argument for port");
                    if(!ushort.TryParse(args[++i], out port)) Program.PrintErrorAndHelp($"Invalid port: {args[i]}");
                    has_port = true;
                    break;

                case "d":
                    if(has_timeout) Program.PrintErrorAndHelp("Timeout already set");
                    if(i == args.Length - 1) Program.PrintErrorAndHelp("Missing argument for timeout");
                    if(!ushort.TryParse(args[++i], out timeout)) Program.PrintErrorAndHelp($"Invalid timeout: {args[i]}");
                    has_timeout = true;
                    break;

                case "-r":
                    if(has_retransmissions) Program.PrintErrorAndHelp("Retransmissions already set");
                    if(i == args.Length - 1) Program.PrintErrorAndHelp("Missing argument for retransmissions");
                    if(!byte.TryParse(args[++i], out retransmissions)) Program.PrintErrorAndHelp($"Invalid retransmissions: {args[i]}");
                    has_retransmissions = true;
                    break;

                case "-h":
                    Program.PrintHelp();
                    break; // Will never get here anyway
            }

        }

        // Check if all required arguments are set
        if(protocol == NetworkProtocol.NONE || hostname == null) Program.PrintErrorAndHelp("Protocol and hostname must be set");
    }
}