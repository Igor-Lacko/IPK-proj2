/* Contains the REPLY message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the REPLY (and negative reply, !REPLY) message.
/// </summary>
public class ReplyMessage : Message
{
    /// <summary>
    /// Regular expression for the textual version of the REPLY message.
    /// </summary>
    public const string Format = @$"^REPLY (OK|NOK) IS (?<MESSAGE_CONTENT>{ParameterFormats.MESSAGE_CONTENT})$";

    /// <summary>
    /// If this is true, it indicates that the message is a positive reply, negative otherwise.
    /// </summary>
    public readonly bool OK;

    /// <summary>
    /// Content of the reply message.
    /// </summary>
    public readonly string MessageContent;

    /// <summary>
    /// ID of the message we are replying to. Only used in the UDP variant.
    /// </summary>
    public readonly ushort RefMessageID;

    /// <summary>
    /// Textual protocol constructor for REPLY message.
    /// </summary>
    /// <param name="ok">True if the message is a positive reply, false otherwise.</param>
    /// <param name="messageContent">Content of the reply message as a string.</param>
    /// <param name="refMessageID">Only used in the UDP variant. ID of the message we are replying to.</param>
    public ReplyMessage(bool ok, string messageContent, ushort refMessageID = 0) : base(MessageType.REPLY)
    {
        OK = ok;
        MessageContent = messageContent;
        RefMessageID = refMessageID;
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
    public override bool IsValid(State clientState) => clientState == State.AUTH || clientState == State.JOIN;

    /// <summary>
    /// Converts the message to a byte array.
    /// |0x01|MessageID|OK|RefMessageID|0|MessageContent|0|
    /// </summary>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes()
    {
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)MessageID!));
        byte ok = (byte)(OK ? 1 : 0);
        byte[] refMessageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)RefMessageID));
        byte[] messageContentBytes = Encoding.ASCII.GetBytes(MessageContent);
        return [(byte)MessageType.REPLY, ok, .. messageIDBytes, .. refMessageIDBytes, 0, .. messageContentBytes, 0];
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"REPLY {(OK ? "OK" : "NOK")} IS {MessageContent}\r\n";
}