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
    public CancellationTokenSource ServerInputCancellationToken { get; } = new();

    /// <summary>
    /// Constructor for TCPServerCommunicator.
    /// </summary>
    /// <param name="host">IP address of the host.</param>
    /// <param name="port">The host port to be connected to.</param>
    public TCPServerCommunicator(IPAddress host, ushort port)
    {
        TCPSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Try to connect
        try
        {
            TCPSocket.Connect(host, port);
        }

        catch (SocketException e)
        {
            Console.WriteLine($"ERROR: Connection unsuccessful: {e.Message}");
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
    public async Task SendMessage(Message message)
    {
        // Check if the message is null
        if (message == null) return;

        // Send the message to the server
        await TCPWriter.WriteAsync(message.ToString());
    }

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
            if(await TCPReader.ReadAsync(current, 0, 1) == 0)
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
        TCPWriter.Close();
        TCPReader.Close();
        TCPStream.Close();
        TCPSocket.Shutdown(SocketShutdown.Both);
        TCPSocket.Close();
    }
}