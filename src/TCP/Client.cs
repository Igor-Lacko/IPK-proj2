/* This file contains the implementation of the main Client class for the TCP variant */

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using src.Common;
using src.TCP;


class TCPClient
{
    // Properties
    public IPAddress DstAddress { get; }                                            // Destination address
    public ushort DstPort { get; }                                                  // Destination port 
    private ClientState State { get; set; }                                         // Client state
    public Socket ClientSocket { get; }                                             // Client socket
    private TextWriter Writer { get; }                                              // Writer
    private TextReader Reader { get; }                                              // Reader

    // Constructor
    public TCPClient(CommandLineArguments arguments)
    {
        // Set the client state and destination port
        State = ClientState.START;
        DstPort = arguments.port;

        // Parse the address if possible, else map hostname to ip with DNS
        if(!IPAddress.TryParse(arguments.hostname, out _))
        {
            DstAddress = Dns.GetHostAddresses(arguments.hostname!)[0];
        }

        else DstAddress = IPAddress.Parse(arguments.hostname);

        // Create the client socket
        ClientSocket = new(DstAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Connect and create a stream
        InitiateConnection();
        NetworkStream stream = new(ClientSocket);

        // Reader/writer
        Writer = new StreamWriter(stream);
        Reader = new StreamReader(stream);
    }

    // Public methods
    public async Task RunAsync()
    {
        do
        {

        } while (State != ClientState.END);
    }

    // Private methods


    private void InitiateConnection()
    {
        // Server endpoint
        IPEndPoint server = new(DstAddress, DstPort);
        ClientSocket.Connect(server);
    }

    /* Async server input/user input handlers */
    private async Task<TCPMessage> ReadMessage()
    {
        // Read and return the message
        string? message = await Reader.ReadLineAsync();
        return new TCPMessage(message);
    }

    private static async Task<TCPCommand> GetUserInput()
    {
        // Get user input
        string? command = await Console.In.ReadLineAsync();
        return new TCPCommand(command);
    }

    /* State handlers */
    private async Task StartState()
    {
        // Get the user/server input (whatrver comes first)
        Task<TCPCommand> input_task = GetUserInput();
        Task<TCPMessage> server_task = ReadMessage();

        // Wait for either to finish
        Task action = await Task.WhenAny(input_task, server_task);

        // Got a command from the user. Represents the AUTH/BYE transitions
        if(action == input_task)
        {
            TCPCommand command = await input_task;
            switch(command.Type)
            {
                default:
                    throw new NotImplementedException("Invalid command (yet to be implemented)");
            }
        }
    }

}