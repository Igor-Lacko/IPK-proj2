namespace IPK_25_CHAT.IO;

/// <summary>
/// Class that runs in the background, reading user input and notifying the client.
/// </summary>
public class UserInputReader()
{
    /// <summary>
    /// Cancellation token source. Cancels the user input reader at the end of the program.
    /// </summary>
    private readonly CancellationTokenSource UserInputCancellationTokenSource = new();

    /// <summary>
    /// Raised on receiving user input.
    /// </summary>
    public event Action<string?> UserInputReceived = str => { };

    /// <summary>
    /// Raised after receiving EOF
    /// </summary>
    public event Action EofReceived = () => { };

    /// <summary>
    /// Closes the input reader.
    /// </summary>
    public void Close() => UserInputCancellationTokenSource.Cancel();

    /// <summary>
    /// "Main" method of the input reader. Is run until the Close() method is called (by the client).
    /// </summary>
    public void Run() => Task.Run(() =>
    {
        while (!UserInputCancellationTokenSource.Token.IsCancellationRequested)
        {
            // Parsing is done separately
            string? input = Console.ReadLine();
            UserInputReceived.Invoke(input);
        }
    });
}