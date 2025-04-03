/* Contains a common interface for TCP/UDP communication with the server */

namespace IPK_25_CHAT.Interface;

using IPK_25_CHAT.Message;

/// <summary>
/// Interface for classes that communicate with the server.
/// </summary>
public interface IServerCommunicator
{
    /// <summary>
    /// Event thrown when a message is received from the server when receiving in a loop.
    /// </summary>
    public event Action<Message?> MessageReceived;

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public void SendMessage(Message message);

    /// <summary>
    /// Reads input from the server.
    /// </summary>
    /// <returns>A Message object representing server input.</returns>
    public Task<Message> ReadInput();

    /// <summary>
    /// Reads input from the server in a loop.
    /// </summary>
    public Task RecieveInputInLoop();

    /// <summary>
    /// Closes the communicator.
    /// </summary>
    public void Close();
}