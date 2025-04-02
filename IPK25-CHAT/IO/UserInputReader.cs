/* Contains the implementation of InputReader class, which asynchronously awaits input from either the user or the server. */

namespace IPK_25_CHAT.IO;

/// <summary>
/// Class that runs in the background, reading user input and notifying the client.
/// </summary>
public class UserInputReader()
{
    /// <summary>
    /// Cancellation token source. Cancels the user input reader at the end of the program.
    /// </summary>
    private readonly CancellationTokenSource UserInputCancellationToken = new();

    /// <summary>
    /// Raised on receiving user input.
    /// </summary>
    public event Action<string?> UserInputReceived = str => { };

    /// <summary>
    /// StreamReader object (for ReadLineAsync).
    /// </summary>
    private readonly StreamReader Reader = new(Console.OpenStandardInput());

    /// <summary>
    /// Closes the input reader.
    /// </summary>
    public void Close()
    {
        UserInputCancellationToken.Cancel();
        Reader.Close();
    }

    /// <summary>
    /// "Main" method of the input reader. Is run until the Close() method is called (by the client).
    /// </summary>
    public void Run() => Task.Run(async () =>
    {
        while (!UserInputCancellationToken.Token.IsCancellationRequested)
        {
            // Parsing is done separately
            string? input = await Reader.ReadLineAsync();
            UserInputReceived.Invoke(input);
        }
    });
}