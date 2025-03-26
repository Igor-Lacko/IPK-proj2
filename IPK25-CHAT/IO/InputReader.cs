/* Contains the implementation of InputReader class, which asynchronously awaits input from either the user or the server. */

namespace IPK_25_CHAT.IO;

using System.Collections.Concurrent;
using System.Net.Sockets;
using IPK_25_CHAT.Command;
using IPK_25_CHAT.Message;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class that asynchronously awaits input from either the user or the server.
/// </summary>
public class InputReader
{
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
    private Queue<IReadable> UserInputQueue;

    /// <summary>
    /// Prevents adding/removing elements from the queue at the same time.
    /// </summary>
    private Semaphore QueueSemaphore;

    /// <summary>
    /// Constructor for InputReader.
    /// </summary>
    /// <param name="serverInputReader">The server input reader for the given type of the Client (TCP/UDP)</param>
    public InputReader(IServerInputReader serverInputReader)
    {
        ServerInputReader = serverInputReader;
        UserInputCancellationToken = new();
        UserInputQueue = new();
        QueueSemaphore = new(0, 1);
    }

    /// <summary>
    /// Reads the next input asynchronously.
    /// </summary>
    /// <returns>The next input, e.g a user command/message or a message from the server.</returns>
    public async Task<IReadable> GetNextInput() => ((Task<IReadable>)await Task.WhenAny(GetUserInput(), ServerInputReader.ReadInput())).Result;

    /// <summary>
    /// Returns the next user input. Waits on a semaphore until the user input is available.
    /// </summary>
    /// <returns>The next user input.</returns>
    private async Task<IReadable> GetUserInput() => await Task.Run(() =>
    {
        QueueSemaphore.WaitOne();
        return UserInputQueue.Dequeue();
    });

    /// <summary>
    /// Closes the input reader.
    /// </summary>
    public void Close() => UserInputCancellationToken.Cancel();

    /// <summary>
    /// Called after receiving user input. Parses the input and enqueues it.
    /// </summary>
    /// <param name="input">String representing the input.</param>
    private void OnUserInputReceived(string input)
    {
        // Valid command
        if(Command.Parse(input, out Command command))
        {
            UserInputQueue.Enqueue(command);
            QueueSemaphore.Release();
        }

        // Invalid command
        else if(input.StartsWith('/'))
            Console.WriteLine($"ERROR: {input}");

        // Message to send to the server
        else throw new NotImplementedException();
    }

    public void Run()
    {
        throw new NotImplementedException();
    }
}