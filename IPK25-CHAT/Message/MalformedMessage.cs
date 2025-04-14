/* Contains the class for a malformed message. Used for local client errors. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class for a malformed message.
/// </summary>
/// <param name="messageContent">Content of the message.</param>
public class MalformedMessage(string? messageContent) : Message(MessageType.MALFORMED)
{
    /// <summary>
    /// Message content.
    /// TODO: Byte version maybe?
    /// </summary>
    public readonly string? MessageContent = messageContent;

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
    /// Throws an exception, since malformed messages do not have message IDs.
    /// </summary>
    /// <throws exception cref="ArgumentException">Always thrown.</exception>
    public override ushort GetMessageID() => throw new ArgumentException("Malformed messages do not have message IDs.");
}