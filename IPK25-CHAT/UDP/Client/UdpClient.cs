/* Contains a class which implements the UDP version of a IPK25CHAT client */

namespace IPK_25_CHAT.UDP.Client;

using IPK_25_CHAT.Client;
using IPK_25_CHAT.IO;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Enum;
using IPK_25_CHAT.Message;
using IPK_25_CHAT.Command;
using System.Net;

/// <summary>
/// Class representing a UDP client.
/// </summary>
public class UDPClient : Client
{
    /// <summary>
    /// Constructor for the UDP client.
    /// </summary>
    /// <param name="host">IP address of the host.</param>
    /// <param name="port">The host port to be connected to.</param>
    public UDPClient(IPAddress host, ushort port) : base(host, port)
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override async Task TerminatingMessageReceived(Message message)
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override async Task ExecuteAuthCommand(AuthCommand command)
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override async Task ExecuteJoinCommand(JoinCommand command)
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override async Task AuthState()
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override async Task OpenState()
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override async Task JoinState()
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override void EndState()
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override IServerCommunicator CreateServerCommunicator()
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override void GracefulTermination()
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }

    protected override async Task ErrorExit(bool sendErrorMessage, string? errorMessage, bool terminateConnection, int exitCode = 1)
    {
        throw new NotImplementedException("UDP client is not implemented yet.");
    }
}