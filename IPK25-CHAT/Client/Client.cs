/* Contains an abstract class for a client. */

namespace IPK_25_CHAT.Client;

using System.Net;
using System.Net.Sockets;
using IPK_25_CHAT.Enum;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.IO;

/// <summary>
/// Abstract base class representing a client instance.
/// </summary>
public abstract class Client
{
    /// <summary>
    /// Client's current state.
    /// </summary>
    protected State State = State.START;

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
    /// Thrown by the client when it's state changes. Useful for example
    /// for the input validator to be able to react to invalid messages
    /// for a given state, e.g. they would otherwise be valid.
    /// </summary>
    public event Action<State> StateChanged = state => { };

    /// <summary>
    /// Thrown by the client when thje user settings (e.g. username, display name change).
    /// Used by the input validator for constructing Msg messages.
    /// </summary>
    public event Action<UserSessionConfiguration> UserSessionChanged = config => { };

    /// <summary>
    /// Current session settings (username, etc.).
    /// </summary>
    private UserSessionConfiguration Config = new();

    /// <summary>
    /// Throws events subscribed to by the client on receiving user input.
    /// </summary>
    private readonly UserInputReader UserInputReader = new();

    /// <summary>
    /// Throws events subscribed to by the client on receiving server input.
    /// </summary>
    private readonly IServerInputReader ServerInputReader;

    /// <summary>
    /// Queue of user inputs.
    /// </summary>
    private readonly UserInputQueue InputQueue = new();

    /// <summary>
    /// Constructor. Sets the host and port.
    /// </summary>
    /// <param name="host">IP address of the server.</param>
    /// <param name="port">Port number of the server.</param>
    public Client(IPAddress host, ushort port)
    {
        // Instance attributes
        Host = host;
        Port = port;
        ClientSocket = CreateSocket();
        ServerInputReader = CreateServerInputReader();

        // Subscribe to events
        UserInputReader.UserInputReceived += OnUserInputReceived;
    }

    /// <summary>
    /// Called after the UserInputReceived event is raised in the UserInputReader class.
    /// Enqueues the given input.
    /// </summary>
    /// <param name="input">The given input from the user.</param>
    private void OnUserInputReceived(string input) => InputQueue.Enqueue(input);

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

    /// <summary>
    /// Creates the socket for sending and receiving messages.
    /// </summary>
    /// <returns>Initialized socket object with (AddressFamily.InterNetwork, SockType.Stream|Dgram, ProtocolType.Tcp|Udp)</returns>
    protected abstract Socket CreateSocket();

    /// <summary>
    /// Creates the server input reader for the given type of the Client (TCP/UDP).
    /// </summary>
    protected abstract IServerInputReader CreateServerInputReader();
}