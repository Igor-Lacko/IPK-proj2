/* Contains the REPLY message. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the REPLY (and negative reply, !REPLY) message.
/// </summary>
public class ReplyMessage : Message
{
    /// <summary>
    /// If this is true, it indicates that the message is a positive reply, negative otherwise.
    /// </summary>
    private readonly bool OK;

    /// <summary>
    /// Content of the reply message.
    /// </summary>
    private readonly string MessageContent;

    /// <summary>
    /// Textual protocol constructor for REPLY message.
    /// </summary>
    /// <param name="ok">True if the message is a positive reply, false otherwise.</param>
    /// <param name="messageContent">Content of the reply message as a string.</param>
    public ReplyMessage(bool ok, string messageContent) : base(MessageType.REPLY)
    {
        OK = ok;
        MessageContent = messageContent;
    }

    /// <summary>
    /// Binary protocol constructor for REPLY message.
    /// </summary>
    /// <param name="messageID">ID of the message as a ushort.</param>
    /// <param name="ok">Byte value indicating whether the reply is positive (1) or negative (0).</param>
    /// <param name="messageContent">Content of the reply message as a byte array.</param>
    public ReplyMessage(ushort messageID, byte[] ok, byte[] messageContent) : base(MessageType.REPLY)
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
    public override string ToString() => $"REPLY {(OK ? "OK" : "NOK")} IS {MessageContent}\r\n";
}