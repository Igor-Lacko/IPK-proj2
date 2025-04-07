/* Contains the ERR message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the ERR message.
/// </summary>
public class ErrMessage : Message
{
    /// <summary>
    /// Regular expression for the textual version of the ERR message.
    /// </summary>
    public const string Format = @$"^ERR FROM (?<DISPLAY_NAME>{ParameterFormats.DISPLAY_NAME}) IS (?<MESSAGE_CONTENT>{ParameterFormats.MESSAGE_CONTENT})$";
    /// <summary>
    /// Error message.
    /// </summary>
    public readonly string DisplayName;

    /// <summary>
    /// Content of the error message.
    /// </summary>
    public readonly string MessageContent;

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
    public override bool IsValid(State clientState) => true;

    /// <summary>
    /// Converts the message to a byte array.
    /// |0xFE|MessageID|DisplayName|0|MessageContent|0|
    /// </summary>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes()
    {
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)MessageID!));
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
        byte[] messageContentBytes = Encoding.ASCII.GetBytes(MessageContent);
        return [(byte)MessageType.ERR, .. messageIDBytes, .. displayNameBytes, 0, .. messageContentBytes, 0];
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"ERR FROM {DisplayName} IS {MessageContent}\r\n";
}