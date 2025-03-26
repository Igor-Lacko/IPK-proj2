/* Contains the CONFIRM message. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the CONFIRM message. This message is UDP specific, and as such is only used in binary form.
/// </summary>
/// <param name="messageID">ID of the message as a ushort.</param>
public class ConfirmMessage : Message
{
    /// <summary>
    /// Constructor for the CONFIRM message.
    /// </summary>
    /// <param name="MessageID">Message ID as a ushort.</param>
    public ConfirmMessage(ushort MessageID) : base(MessageType.CONFIRM)
    {
        this.MessageID = MessageID;
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
    public override byte[] AsBytes() => [.. (byte[])[(byte)MessageType.CONFIRM], .. BitConverter.GetBytes((ushort)MessageID!)];

    /// <summary>
    /// Throws an exception as the CONFIRM message is not used in textual form.
    /// </summary>
    /// <returns>Throws an exception.</returns>
    /// <exception cref="ArgumentException">Thrown when the method is called.</exception>
    public override string AsString() => throw new ArgumentException("CONFIRM message is not used in textual form.");
}