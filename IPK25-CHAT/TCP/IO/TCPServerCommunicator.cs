/* Class for communication with the server via TCP. Uses a NetworkStream object. */

namespace IPK_25_CHAT.TCP.IO;

using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Message;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Class for communication with the server via TCP.
/// Uses a network stream and text based messages.
/// </summary>
public class TCPServerCommunicator : IServerCommunicator
{
    /// <summary>
    /// Socket of type (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp).
    /// </summary>
    private readonly Socket TCPSocket;

    /// <summary>
    /// NetworkStream object for reading and writing data.
    /// </summary>
    private readonly NetworkStream TCPStream;

    /// <summary>
    /// StreamReader object for reading data from the network stream.
    /// </summary>
    private readonly StreamReader TCPReader;

    /// <summary>
    /// StreamWriter object for writing data to the network stream.
    /// </summary>
    private readonly StreamWriter TCPWriter;

    /// <summary>
    /// Event thrown when a message is received from the server when receiving in a loop.
    /// </summary>
    public event Action<Message> MessageReceived = message => { };

    /// <summary>
    /// Cancellation token source. Cancels the server communicator at the end of the program.
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <summary>
    /// Constructor for TCPServerCommunicator.
    /// </summary>
    /// <param name="host">IP address of the host.</param>
    /// <param name="port">The host port to be connected to.</param>
    public TCPServerCommunicator(IPAddress host, ushort port)
    {
        TCPSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        TCPSocket.Connect(host, port);
        TCPStream = new(TCPSocket);
        TCPReader = new(TCPStream);
        TCPWriter = new(TCPStream);
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public void SendMessage(Message message) => TCPWriter.WriteLine(message.ToString());

    /// <summary>
    /// Reads input from the server.
    /// </summary>
    /// <returns>A Message object representing server input.</returns>
    public async Task<Message?> ReadInput()
    {
        string? input = await TCPReader.ReadLineAsync();
        if (input == null) return null;

        // Try to parse the message
        else if(Message.Parse(input, out Message message))
            return message;

        else return null;
    }

    /// <summary>
    /// Reads input from the server in a loop.
    /// </summary>
    public async Task RecieveInputInLoop()
    {
        while (!CancellationTokenSource.Token.IsCancellationRequested)
        {
            // Read input from the server
            Message? message = await ReadInput();

            // If the message is null, break the loop
            if (message == null) break;

            // Invoke the event
            MessageReceived.Invoke(message);
        }
    }

    /// <summary>
    /// Closes the communicator.
    /// </summary>
    public void Close()
    {
        CancellationTokenSource.Cancel();
        TCPWriter.Close();
        TCPReader.Close();
        TCPStream.Close();
        TCPSocket.Shutdown(SocketShutdown.Both);
        TCPSocket.Close();
    }
}