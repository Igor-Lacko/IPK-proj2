/* Contains a common interface for TCP/UDP communication with the server */

namespace IPK_25_CHAT.Interface;

using IPK_25_CHAT.Message;

/// <summary>
/// Interface for classes that communicate with the server.
/// </summary>
public interface IServerCommunicator
{
    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public void SendMessage(Message message);

    /// <summary>
    /// Closes the communicator.
    /// </summary>
    public void Close();

    /// <summary>
    /// Reads input from the server.
    /// </summary>
    /// <returns>A Message object representing server input.</returns>
    public Task<Message> ReadInput();

    /// <summary>
    /// Event thrown on receiving an ERR message.
    /// </summary>
    public event Action<ErrMessage> ErrMessageReceived;

    /// <summary>
    /// Event thrown on receiving a BYE message.
    /// </summary>
    public event Action ByeMessageReceived;

    /// <summary>
    /// Event thrown on receiving a malformed message.
    /// </summary>
    public event Action<string> MalformedMessageReceived;
}