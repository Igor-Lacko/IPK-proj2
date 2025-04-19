namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

using System.Net;
using System.Text;

/// <summary>
/// Class representing the REPLY (and negative reply, !REPLY) message.
/// </summary>
/// <param name="ok">True if the message is a positive reply, false otherwise.</param>
/// <param name="messageContent">Content of the reply message as a string.</param>
/// <param name="messageID">ID of the message. Only used in the UDP variant, hence the default value.</param>
/// <param name="refMessageID">Same as messageID, identifies the message that is being replied to.</param>
public class ReplyMessage(bool ok, string messageContent, ushort messageID = 0, ushort refMessageID = 0) : Message(MessageType.REPLY)
{
    /// <summary>
    /// ID of the message.
    /// </summary>
    private ushort MessageID = messageID;

    /// <summary>
    /// Regular expression for the textual version of the REPLY message.
    /// </summary>
    public const string Format = @$"(?i)^REPLY (OK|NOK) IS (?<MESSAGE_CONTENT>{ParameterFormats.MESSAGE_CONTENT})$";

    /// <summary>
    /// If this is true, it indicates that the message is a positive reply, negative otherwise.
    /// </summary>
    public readonly bool OK = ok;

    /// <summary>
    /// Content of the reply message.
    /// </summary>
    public readonly string MessageContent = messageContent;

    /// <summary>
    /// ID of the message we are replying to. Only used in the UDP variant.
    /// </summary>
    public readonly ushort RefMessageID = refMessageID;

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
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes(ushort messageID)
    {
        MessageID = messageID;

        // Message fields
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)MessageID));
        byte ok = (byte)(OK ? 1 : 0);
        byte[] refMessageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)RefMessageID));
        byte[] messageContentBytes = Encoding.ASCII.GetBytes(MessageContent);

        // Serialize into a byte array
        return [(byte)MessageType.REPLY, ok, .. messageIDBytes, .. refMessageIDBytes, 0, .. messageContentBytes, 0];
    }

    /// <summary>
    /// Tries to parse a REPLY message from a byte array.
    /// </summary>
    /// <param name="response">Byte array representing the message.</param>
    /// <param name="bytesReceived">Number of bytes received.</param>
    /// <param name="message">The resulting REPLY message if parsed successfully, else null.</param>
    /// <returns>True if parsed successfully, else false.</returns>
    public static bool TryParse(byte[] response, int bytesReceived, out ReplyMessage? message)
    {
        // Has to be at least |0x01|MessageID|MessageID|OK|RefMessageID|RefMessageID|MessageContent|0|
        if(bytesReceived < 8)
        {
            message = null;
            return false;
        }

        // Message ID
        ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 1));

        // Result
        bool ok;
        if (response[3] == 1)
            ok = true;

        else if (response[3] == 0)
            ok = false;

        else
        {
            message = null;
            return false;
        }

        // RefMessageID
        ushort refMessageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 4));

        // Try to parse the message content
        int messageContentEnd = ParseMessageContent(response, 6, out string? messageContent);
        if (messageContent == null || messageContentEnd != bytesReceived)
        {
            message = null;
            return false;
        }

        // Create and return the message
        message = new ReplyMessage(ok, messageContent!, messageID, refMessageID);
        return true;
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"REPLY {(OK ? "OK" : "NOK")} IS {MessageContent}\r\n";

    /// <summary>
    /// Returns the message ID (of the REPLY itself, not the referenced message).
    /// </summary>
    /// <returns>Message ID of the REPLY message.</returns>
    public override ushort GetMessageID() => MessageID;
}