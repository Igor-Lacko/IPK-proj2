/* Contains the implementation of InputReader class, which asynchronously awaits input from either the user or the server. */

using System.Net.Sockets;

namespace src.IO;

using System.Collections.Concurrent;
using src.Interface;

/// <summary>
/// Class that asynchronously awaits input from either the user or the server.
/// </summary>
public class InputReader
{
    /// <summary>
    /// Socket to receive messages from/convert to a network stream.
    /// </summary>
    private Socket ReceiveSocket;

    /// <summary>
    /// StreamReader to read user input from the console.
    /// </summary>
    private StreamReader UserInputReader;

    /// <summary>
    /// Reads server input asynchronously.
    /// </summary>
    private IServerInputReader ServerInputReader;

    /// <summary>
    /// Cancellation token source. Cancels the user input reader at the end of the program.
    /// </summary>
    private CancellationTokenSource UserInputCancellationToken;

    /// <summary>
    /// Queue of user inputs.
    /// </summary>
    private ConcurrentQueue<IReadable> UserInputQueue;

    /// <summary>
    /// Closes the input reader.
    /// </summary>
    public void Close()
    {
        UserInputCancellationToken.Cancel();
    }
}