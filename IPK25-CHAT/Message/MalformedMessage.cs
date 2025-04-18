/* Contains the class for a malformed message. Used for local client errors. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class for a malformed message.
/// </summary>
public class MalformedMessage(ushort? messageId) : Message(MessageType.MALFORMED)
{
    /// <summary>
    /// Message ID. May or may not have one (has one if bytes received >= 3).
    /// </summary>
    public ushort? MessageID = messageId;

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// Always returns true to be handled differently than other messages.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True.</returns>
    public override bool IsValid(State clientState) => true;

    /// <summary>
    /// Converts the message to a byte array.
    /// </summary>
    /// <returns>Byte array representation of the message.</returns>
    public override byte[] AsBytes(ushort messageID)
    {
        throw new ArgumentException("Malformed message cannot be converted to bytes.");
    }

    /// <summary>
    /// Throws an exception, since we need to access the ID from the public attribute (it is nullable as opposed to other messages).
    /// </summary>
    /// <throws exception cref="ArgumentException">Always thrown.</exception>
    public override ushort GetMessageID() => throw new ArgumentException("Acess message id through the public attribute for malformed messages!");
}