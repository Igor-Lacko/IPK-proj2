namespace IPK_25_CHAT.UDP;

using IPK_25_CHAT.Client;
using IPK_25_CHAT.IO;
using IPK_25_CHAT.Message;
using IPK_25_CHAT.Command;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Enum;

using System.Net;

/// <summary>
/// Class representing a UDP client.
/// </summary>
public class UDPClient : Client
{
    /// <summary>
    /// Number of retransmissions for the UDP client.
    /// </summary>
    private readonly ushort NumberOfRetransmissions;

    /// <summary>
    /// Timeout when waiting for CONFIRM
    /// </summary>
    private readonly ushort Timeout;

    /// <summary>
    /// Flag indicating if a sent message timeouted.
    /// </summary>
    private bool Timeouted;

    /// <summary>
    /// Constructor for the UDP client.
    /// </summary>
    /// <param name="host">IP address of the host.</param>
    /// <param name="port">The host port to be connected to.</param>
    /// <param name="numberOfRetransmissions">Number of retries until a message is confirmed.</param>
    /// <param name="timeout">Timeout for one confirm attempt.</param>
    public UDPClient(IPAddress host, ushort port, ushort numberOfRetransmissions, ushort timeout) : base(host, port)
    {
        NumberOfRetransmissions = numberOfRetransmissions;
        Timeout = timeout;
        ServerCommunicator = CreateServerCommunicator();

        // Max message content size --> 1500 - (TYPE(1) + ID(2) + DNAME(20) + 1 + 1) = 1475 (I hope this is correct)
        InputValidator.MaxMessageLength = 1475;

        // Subscribe to events
        ServerCommunicator.MessageReceived += ServerInputQueue.Enqueue;
        ((UDPServerCommunicator)ServerCommunicator).ConfirmTimeouted += OnMessageTimeout;
    }

    /// <summary>
    /// Method to be called when a message times out.
    /// </summary>
    private void OnMessageTimeout()
    {
        // Local error
        StdoutResultWriter.InternalClientError("Message did not receive a CONFIRM in time!");

        // For the other methods
        Timeouted = true;

        // Terminate the connection
        ErrorExit(false, null, ExitCodes.UDP_CONFIRM_TIMEOUT).Wait();
    }

    /// <summary>
    /// Override of OnErrReceived. Doesn't print out the message, done by the communicator already.
    /// </summary>
    /// <param name="message">The err message (for compatibility with the base class). Ignored.</param>
    protected override async Task OnErrMessageReceived(ErrMessage message)
    {
        await ErrorExit(false, null, ExitCodes.ERR_RECEIVED);
    }

    /// <summary>
    /// Executes the AUTH command. Same as the base/TCP version, just checks for timeouts.
    /// </summary>
    protected override async Task ExecuteAuthCommand(AuthCommand command)
    {
        await base.ExecuteAuthCommand(command);
        if(Timeouted) UpdateState(State.END);
    }

    /// <summary>
    /// Executes the OPEN command. Same as the base/TCP version, just checks for timeouts.
    /// </summary>
    protected override async Task ExecuteJoinCommand(JoinCommand command)
    {
        await base.ExecuteJoinCommand(command);
        if(Timeouted) UpdateState(State.END);
    }

    /// <summary>
    /// Creates the server communicator for the UDP client.
    /// </summary>
    /// <returns>The communicator object.</returns>
    protected override IServerCommunicator CreateServerCommunicator() => new UDPServerCommunicator(Host, Port, Timeout, NumberOfRetransmissions);

    /// <summary>
    /// Handles the user input.
    /// </summary>
    /// <param name="input">The user input.</param>
    protected override async Task RunUserInput(IReadable input)
    {
        // Run the input
        await base.RunUserInput(input);

        // Check timeouts
        if(Timeouted) UpdateState(State.END);
    }
}