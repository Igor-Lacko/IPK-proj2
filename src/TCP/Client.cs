/* This file contains the implementation of the main Client class for the TCP variant */

using src.Common;

class TCPClient(CommandLineArguments arguments)
{
    // Properties
    public string Hostname { get; } = arguments.hostname;
    public ushort Port { get; } = arguments.port;

    public void Run()
    {
        throw new NotImplementedException("TCP client not yet implemented");
    }
}