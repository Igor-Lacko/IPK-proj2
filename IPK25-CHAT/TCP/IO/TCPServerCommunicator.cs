/* Class for communication with the server via TCP. Uses a NetworkStream object. */

namespace IPK_25_CHAT.TCP.IO;

using IPK_25_CHAT.Interface;
using IPK_25_CHAT.IO;
using IPK_25_CHAT.Message;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Class for communication with the server via TCP.
/// Uses a network stream and text based messages.
/// </summary>
/// <remarks>
/// Constructor for TCPServerCommunicator.
/// </remarks>
/// <param name="host">IP address of the host.</param>
/// <param name="port">The host port to be connected to.</param>
public class TCPServerCommunicator(IPAddress host, ushort port) : IServerCommunicator
{
    /// <summary>
    /// Socket of type (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp).
    /// </summary>
    private Socket? TCPSocket;

    /// <summary>
    /// NetworkStream object for reading and writing data.
    /// </summary>
    private NetworkStream? TCPStream = null;

    /// <summary>
    /// StreamReader object for reading data from the network stream.
    /// </summary>
    private StreamReader? TCPReader = null;

    /// <summary>
    /// StreamWriter object for writing data to the network stream.
    /// </summary>
    private StreamWriter? TCPWriter = null;

    /// <summary>
    /// IP address of the host.
    /// </summary>
    private readonly IPAddress Host = host;

    /// <summary>
    /// Port of the host.
    /// </summary>
    private readonly ushort Port = port;

    /// <summary>
    /// Event thrown when a message is received from the server when receiving in a loop.
    /// </summary>
    public event Action<Message> MessageReceived = message => { };

    /// <summary>
    /// Cancellation token source. Cancels the server communicator at the end of the program.
    /// </summary>
    public CancellationTokenSource ServerInputCancellationToken { get; } = new();

    /// <summary>
    /// Opens connection to the server.
    /// </summary>
    /// <throws>SocketException if the connection fails.</throws>
    public void Initialize()
    {
        // Create the socket
        TCPSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Try to connect
        try
        {
            TCPSocket.Connect(Host, Port);
        }

        catch (SocketException e)
        {
            StdoutResultWriter.InternalClientError($"Could not connect to the server: {e.Message}");
            Environment.Exit(1);
        }

        // Initialize the stream
        TCPStream = new(TCPSocket);
        TCPReader = new(TCPStream, System.Text.Encoding.ASCII);
        TCPWriter = new(TCPStream, System.Text.Encoding.ASCII) { AutoFlush = true };
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public async Task SendMessage(Message message) => await TCPWriter!.WriteAsync(message.ToString());

    /// <summary>
    /// Reads one message from the server.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public async Task<string> GetMessage()
    {
        List<char> message = [];
        char[] current = new char[1];
        int currentIndex = 0;

        // Until \r\n
        bool done = false;

        while (!done)
        {
            // Read one character
            int bytesRead = await TCPReader!.ReadAsync(current, 0, 1);

            if (bytesRead == 0)
                break;

            else message.Add(current[0]);

            // If we have reached the end
            if(current[0] == '\n')
            {
                // Check if the last character is \r
                if (currentIndex != 0 && message[currentIndex - 1] == '\r')
                    done = true;
            }

            currentIndex++;
        }

        // Strip the \r\n from the message
        if(done)
        {
            message.RemoveAt(currentIndex - 1);
            message.RemoveAt(currentIndex - 2);
        }

        return new string([.. message]);
    }

    /// <summary>
    /// Reads input from the server.
    /// </summary>
    public void Run() => Task.Run(async () =>
    {
        while (!ServerInputCancellationToken.Token.IsCancellationRequested)
        {
            // Parsing is done separately
            string input = await GetMessage();
            Message message = Message.Parse(input);
            MessageReceived.Invoke(message);
        }
    });

    /// <summary>
    /// Closes the communicator.
    /// </summary>
    public void Close()
    {
        ServerInputCancellationToken.Cancel();
        TCPWriter!.Close();
        TCPReader!.Close();
        TCPStream!.Close();
        TCPSocket!.Shutdown(SocketShutdown.Both);
        TCPSocket.Close();
    }
}