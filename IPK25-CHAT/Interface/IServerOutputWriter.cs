/* Contains a common interface for classes that send messages to server. */

namespace IPK_25_CHAT.Interface;

/// <summary>
/// Interface for classes that send messages to the server.
/// </summary>
public interface IServerOutputWriter
{
    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public void SendMessage(string message);
}