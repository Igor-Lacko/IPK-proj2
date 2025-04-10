/* Contains the MSG message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the MSG message.
/// </summary>
/// <param name="displayName">Display name of the user as a string.</param>
/// <param name="messageContent">Content of the message as a string.</param>
/// <param name="messageID">ID of the message as a byte. Unused in the TCP variant, hence the default value.</param>
public class MsgMessage(string displayName, string messageContent, ushort messageID = 0) : Message(MessageType.MSG)
{
    /// <summary>
    /// ID of the message.
    /// </summary>
    private ushort MessageID = messageID;

    /// <summary>
    /// Regular expression for the textual version of the MSG message.
    /// </summary>
    public const string Format = @$"^MSG FROM (?<DISPLAY_NAME>{ParameterFormats.DISPLAY_NAME}) IS (?<MESSAGE_CONTENT>{ParameterFormats.MESSAGE_CONTENT})$";

    /// <summary>
    /// Display name of the user.
    /// </summary>
    public readonly string DisplayName = displayName;

    /// <summary>
    /// Content of the message.
    /// </summary>
    public readonly string MessageContent = messageContent;

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public override bool IsValid(State clientState) => clientState == State.OPEN || clientState == State.JOIN;

    /// <summary>
    /// Converts the message to a byte array.
    /// |0x04|MessageID|DisplayName|0|MessageContent|0|
    /// </summary>
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes(ushort messageID)
    {
        // Set ID
        MessageID = messageID;

        // Message fields
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)messageID));
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
        byte[] messageContentBytes = Encoding.ASCII.GetBytes(MessageContent);
        
        // Serialize into a byte array
        return [(byte)MessageType.MSG, .. messageIDBytes, .. displayNameBytes, 0, .. messageContentBytes, 0];
    }

    /// <summary>
    /// Tries to parse a MSG message from a byte array.
    /// </summary>
    /// <param name="response">Byte array representing the message.</param>
    /// <param name="message">The resulting MSG message if parsed successfully, else null.</param>
    /// <returns>True if parsed successfully, else false.</returns>
    public static bool TryParse(byte[] response, out MsgMessage? message)
    {
        // Message ID
        ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 1));

        // Try to parse the display name
        int messageContentIndex = ParseDisplayName(response, 3, out bool success, out string? displayName);
        if(!success)
        {
            message = null;
            return false;
        }

        // Try to parse the message content
        string? messageContent = ParseMessageContent(response, messageContentIndex, out success);
        if(!success)
        {
            message = null;
            return false;
        }

        // Create and return the message
        message = new MsgMessage(displayName!, messageContent!, messageID);
        return true;
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"MSG FROM {DisplayName} IS {MessageContent}\r\n";

    /// <summary>
    /// Returns the message id. Needed for compatibility with the Message class.
    /// </summary>
    /// <returns>Message ID.</returns>
    public override ushort GetMessageID() => MessageID;
}