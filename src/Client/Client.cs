/* Contains an abstract class for a client. */

namespace src.Client;

using System.Net;
using System.Net.Sockets;
using src.Enum;

/// <summary>
/// Abstract base class representing a client instance.
/// </summary>
public abstract class Client
{
    /// <summary>
    /// Client's current state.
    /// </summary>
    protected State State;

    /// <summary>
    /// IP address of the server.
    /// </summary>
    protected IPAddress Host;

    /// <summary>
    /// Port number of the server.
    /// </summary>
    protected ushort Port;

    /// <summary>
    /// Socket to send from and recieve to.
    /// </summary>
    protected Socket ClientSocket;

    /// <summary>
    /// Constructor. Sets the host and port.
    /// </summary>
    /// <param name="host">IP address of the server.</param>
    /// <param name="port">Port number of the server.</param>
    public Client(IPAddress host, ushort port)
    {
        Host = host;
        Port = port;
        State = State.START;
        ClientSocket = CreateSocket();
    }

    /// <summary>
    /// Creates the socket for sending and receiving messages.
    /// </summary>
    /// <returns>Initialized socket object with (AddressFamily.InterNetwork, SockType.Stream|Dgram, ProtocolType.Tcp|Udp)</returns>
    protected abstract Socket CreateSocket();

    /// <summary>
    /// Main method. Runs the client. Is implemented as a async do-while loop which
    /// handles the current state.
    /// </summary>
    private async Task Run()
    {
        do
        {
            switch(State)
            {
                case State.START:
                    await StartState();
                    break;

                case State.AUTH:
                    await AuthState();
                    break;

                case State.OPEN:
                    await OpenState();
                    break;

                case State.JOIN:
                    await JoinState();
                    break;
            }
        } while(State != State.END);

        await EndState();
    }

    /// <summary>
    /// Handles the client's starting state.
    /// </summary>
    private async Task StartState()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles the client's authentication state.
    /// </summary>
    private async Task AuthState()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles the client's open state.
    /// </summary>
    private async Task OpenState()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles the client's join state.
    /// </summary>
    private async Task JoinState()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles the client's end state.
    /// Might not need to be async?
    /// </summary>
    private async Task EndState()
    {
        throw new NotImplementedException();
    }
}