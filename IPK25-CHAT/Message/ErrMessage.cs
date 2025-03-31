/* Contains the ERR message. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the ERR message.
/// </summary>
public class ErrMessage : Message
{
    /// <summary>
    /// Error message.
    /// </summary>
    private readonly string DisplayName;

    /// <summary>
    /// Content of the error message.
    /// </summary>
    private readonly string MessageContent;

    /// <summary>
    /// Textual protocol constructor for ERR message.
    /// </summary>
    /// <param name="displayName">Display name of the user as a string.</param>
    /// <param name="messageContent">Content of the error message as a string.</param>
    public ErrMessage(string displayName, string messageContent) : base(MessageType.ERR)
    {
        DisplayName = displayName;
        MessageContent = messageContent;
    }

    /// <summary>
    /// Binary protocol constructor for ERR message.
    /// </summary>
    /// <param name="messageID">ID of the message as a ushort.</param>
    /// <param name="displayName">Display name of the user as a byte array.</param>
    /// <param name="messageContent">Content of the error message as a byte array.</param>
    public ErrMessage(ushort MessageID, byte[] displayName, byte[] messageContent) : base(MessageType.ERR)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public override bool IsValid(State clientState)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts the message to a byte array.
    /// </summary>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"ERR FROM {DisplayName} IS {MessageContent}\r\n";
}