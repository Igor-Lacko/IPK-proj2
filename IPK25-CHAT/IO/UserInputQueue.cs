/* Class containing a queue of inputs (messages, commands) */

namespace IPK_25_CHAT.IO;

/// <summary>
/// Provides a interface for interacting with a queue of inputs.
/// </summary>
public class InputQueue<T>
{
    /// <summary>
    /// Queue of inputs.
    /// </summary>
    private readonly Queue<T> InQueue = new();

    /// <summary>
    /// Semaphore controling access to the queue.
    /// </summary>
    private readonly SemaphoreSlim QueueGuardian = new(0, 1);

    /// <summary>
    /// Wrapper on enqueueing input.
    /// </summary>
    /// <param name="input">Input to enqueue.</param>
    public void Enqueue(T input)
    {
        Console.WriteLine("Enqueueing input: " + input);
        InQueue.Enqueue(input);
        QueueGuardian.Release();
    }

    /// <summary>
    /// Wrapper on dequeueing input.
    /// </summary>
    /// <returns>input from the queue.</returns>
    public async Task<T> Dequeue(CancellationToken token)
    {
        try
        {
            Console.WriteLine("Waiting for input...");
            await QueueGuardian.WaitAsync(token);
            return InQueue.Dequeue();
        }

        catch (OperationCanceledException)
        {
            return default!;
        }
    }
}