/* Class for communication with the server via TCP. Uses a NetworkStream object. */

namespace IPK_25_CHAT.TCP;

using IPK_25_CHAT.IO;
using IPK_25_CHAT.Message;
using IPK_25_CHAT.Interface;
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
public class TCPServerCommunicator : IServerCommunicator
{
    /// <summary>
    /// Socket of type (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp).
    /// </summary>
    private readonly Socket TCPSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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
    /// IP address of the host.
    /// </summary>
    private readonly IPAddress Host;

    /// <summary>
    /// Port of the host.
    /// </summary>
    private readonly ushort Port;

    /// <summary>
    /// Event thrown when a message is received from the server when receiving in a loop.
    /// </summary>
    public event Action<Message> MessageReceived = message => { };

    /// <summary>
    /// Cancellation token source. Cancels the server communicator at the end of the program.
    /// </summary>
    public CancellationTokenSource ServerInputCancellationTokenSource { get; } = new();

    public TCPServerCommunicator(IPAddress host, ushort port)
    {
        // Initialize the stream
        Host = host;
        Port = port;

        // Try to connect
        try
        {
            TCPSocket.Connect(Host, Port);
            TCPStream = new(TCPSocket);
            TCPReader = new(TCPStream, System.Text.Encoding.ASCII);
            TCPWriter = new(TCPStream, System.Text.Encoding.ASCII) { AutoFlush = true };
        }

        catch (SocketException e)
        {
            StdoutResultWriter.InternalClientError($"Failed to connect: {e.Message}");
            Environment.Exit(1);
        }
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
        Memory<char> current = new char[1];
        int currentIndex = 0;

        // Until \r\n
        bool done = false;

        while (!done)
        {
            // Read one character
            int bytesRead = await TCPReader!.ReadAsync(current, ServerInputCancellationTokenSource.Token);

            // Check for cancellation
            ServerInputCancellationTokenSource.Token.ThrowIfCancellationRequested();

            if (bytesRead == 0)
                break;

            else message.Add(current.Span[0]);

            // If we have reached the end
            if(current.Span[0] == '\n')
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
        while (!ServerInputCancellationTokenSource.Token.IsCancellationRequested)
        {
            // Parsing is done separately
            try
            {
                string input = await GetMessage();
                Message message = Message.Parse(input);
                MessageReceived.Invoke(message);
            }

            catch(OperationCanceledException)
            {
                return;
            }
        }
    });

    /// <summary>
    /// Closes the communicator.
    /// </summary>
    public void Close()
    {
        ServerInputCancellationTokenSource.Cancel();
        TCPReader.Close();
        TCPWriter.Close();
        TCPStream.Close();
        TCPSocket.Shutdown(SocketShutdown.Both);
        TCPSocket.Close();
    }
}