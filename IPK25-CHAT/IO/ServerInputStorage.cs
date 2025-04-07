/* This class stores the incoming inputs from the server */

namespace IPK_25_CHAT.IO;

using IPK_25_CHAT.Message;

public class ServerInputStorage
{
    /// <summary>
    /// Guards the input from invalid access.
    /// </summary>
    private readonly SemaphoreSlim InputGuardian = new(0);

    /// <summary>
    /// Releases the input guardian once and sets the last message.
    /// </summary>
    public void OnMessageReceived(Message message)
    {
        // Set the last message
        LastMessage = message;

        // Release the input guardian
        InputGuardian.Release();
    }

    /// <summary>
    /// Last message from the server.
    /// </summary>
    private Message? LastMessage = null;

    /// <summary>
    /// Waits for the next input. Then it locks the semaphore.
    /// </summary>
    public async Task<Message> WaitForInput(CancellationToken token)
    {
        try
        {
            // Wait until cancellation
            await InputGuardian.WaitAsync(token);
            return LastMessage!;
        }

        catch (OperationCanceledException)
        {
            return new MalformedMessage("Operation cancelled");
        }
    }
}