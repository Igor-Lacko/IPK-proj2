/* Class containing a queue of user inputs (messages, commands) */

namespace IPK_25_CHAT.IO;

/// <summary>
/// Provides a interface for interacting with a queue of user inputs.
/// </summary>
class UserInputQueue
{
    /// <summary>
    /// Queue of user inputs.
    /// </summary>
    private readonly Queue<string> InputQueue = new();

    /// <summary>
    /// Semaphore controling access to the queue.
    /// </summary>
    private readonly Semaphore QueueGuardian = new(0, 1);

    /// <summary>
    /// Wrapper on enqueueing user input.
    /// </summary>
    /// <param name="input">User input to enqueue.</param>
    public void Enqueue(string input)
    {
        InputQueue.Enqueue(input);
        QueueGuardian.Release();
    }

    /// <summary>
    /// Wrapper on dequeueing user input.
    /// </summary>
    /// <returns>User input from the queue.</returns>
    public async Task<string> Dequeue() => await Task.Run(() => 
    {
        QueueGuardian.WaitOne();
        return InputQueue.Dequeue();
    });
}