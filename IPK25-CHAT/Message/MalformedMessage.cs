/* Contains the class for a malformed message. Used for local client errors. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class for a malformed message.
/// </summary>
public class MalformedMessage : Message
{
    /// <summary>
    /// Message content.
    /// TODO: Byte version maybe?
    /// </summary>
    public readonly string? MessageContent;

    /// <summary>
    /// Constructor for the MalformedMessage class.
    /// </summary>
    public MalformedMessage(string? messageContent) : base(MessageType.MALFORMED)
    {
        MessageID = null;
        MessageContent = messageContent;
    }

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
    public override byte[] AsBytes()
    {
        throw new ArgumentException("Malformed message cannot be converted to bytes.");
    }

    /// <summary>
    /// Converts the message to a string. Used for local client errors.
    /// Will probably not be used, since the message can be null.
    /// </summary>
    /// <returns>String representation of the message.</returns>
    public override string ToString() => MessageContent ?? "Malformed message";
}