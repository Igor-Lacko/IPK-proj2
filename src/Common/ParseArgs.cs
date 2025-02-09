/* This file contains the implementation of parsing CLI arguments. */

namespace src.Common;

class CommandLineArguments
{
    public Program.NetworkProtocol protocol;
    public string hostname = "temp"; // Default value to get rid of warnings, despite incorrect arguments not being tested. TODO: better solution
    public ushort port = 4567;
    public ushort timeout = 250; // UDP only
    public byte retransmissions = 3; // UDP only

    public CommandLineArguments(string[] args)
    {
        ParseArgs(args);
    }

    /// <summary>
    /// Main CLI parsing logic. It assumes that the arguments are correctly provided, as per the assignment. TODO: Add error handling if allowed?
    /// </summary>
    public void ParseArgs(string[] args)
    {
        for(int i = 0; i < args.Length; i++)
        {
            switch(args[i])
            {
                case "-t":
                    protocol = args[++i] == "tcp" ? Program.NetworkProtocol.TCP : Program.NetworkProtocol.UDP;
                    break;

                case "-s":
                    hostname = args[++i];
                    break;

                case "-p":
                    port = ushort.Parse(args[++i]);
                    break;

                case "d":
                    timeout = ushort.Parse(args[++i]);
                    break;

                case "-r":
                    retransmissions = byte.Parse(args[++i]);
                    break;

                case "-h":
                    Program.PrintHelp();
                    break; // Will never get here anyway
            }

            // Move on to the next option
            i++;
        }
    }
}