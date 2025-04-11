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
    /// Waits on the semaphore until the next message arrives and returns it to the client.
    /// </summary>
    /// <param name="token">Cancellation token to cancel the wait. Used if the client is waiting for the
    /// user and server at the same time (else CancellationToken.None) is passed.</param>
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