/* This file contains the implementation of the main Client class for the TCP variant */

using System.Formats.Asn1;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using src.Common;
using src.TCP;


class TCPClient
{
    // Properties
    public IPAddress DstAddress { get; }                                                        // Destination address
    public ushort DstPort { get; }                                                              // Destination port 
    private ClientState State { get; set; }                                                     // Client state
    public Socket ClientSocket { get; }                                                         // Client socket
    private TextWriter ServerSender { get; }                                                    // Message writer
    private TextReader ServerReader { get; }                                                    // Message reader
    private TextReader InputReader { get; } = new StreamReader(Console.OpenStandardInput());    // Input reader                                         
    private ClientData Data { get; set; } = new();                                              // Current data
    private NetworkStream Stream { get; }                                                       // Stream

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
        try
        {
            ClientSocket.Connect(new IPEndPoint(DstAddress, DstPort));
        }

        catch(Exception e)
        {
            ErrorLogger.ErrorMessage($"Failed to connect to the server: {e.Message}");
            Environment.Exit(1);
        }

        Stream = new(ClientSocket);

        // Reader/writer
        ServerSender = new StreamWriter(Stream);
        ServerReader = new StreamReader(Stream);
    }

    // Methods
    public async Task RunAsync()
    {
        // Add CTRL + C handler
        Console.CancelKeyPress += delegate
        {
            TCPMessenger.SendByeMessage(ServerSender, Data);
            State = ClientState.END;
        };

        // Main loop
        do
        {
            switch(State)
            {
                case ClientState.START:
                    await StartAsync();
                    break;

                case ClientState.AUTH:
                    //AuthState();
                    break;

                case ClientState.OPEN:
                    //OpenState();
                    break;

                case ClientState.JOIN:
                    //JoinState();
                    break;
            }
        } while(State != ClientState.END);

        GracefulExit();
    }

    private void GracefulExit()
    {
        ClientSocket.Shutdown(SocketShutdown.Both);
        ClientSocket.Close();
        Environment.Exit(0);
    }

    private async Task StartAsync()
    {

        var user_input = Console.In.ReadLineAsync();
        var server_response = ServerReader.ReadLineAsync();
        Task first_task = await Task.WhenAny(user_input, server_response);

        Console.WriteLine(first_task == user_input ? "User input" : "Server response");
    }
}