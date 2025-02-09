/* This file contains the implenentation of the main Client class for the UDP variant. Not yet implemented */

using src.Common;

namespace src.UDP;



class UDPClient(CommandLineArguments arguments)
{
    // Properties
    public string Hostname { get; } = arguments.hostname;
    public ushort Port { get; } = arguments.port;
    public ushort Timeout { get; } = arguments.timeout;
    public byte Retransmissions { get; } = arguments.retransmissions;


    public void Run()
    {
        throw new NotImplementedException("UDP client not yet implemented");
    }
}