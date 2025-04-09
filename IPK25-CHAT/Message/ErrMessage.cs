/* Contains the ERR message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the ERR message.
/// </summary>
/// <param name="displayName">Display name of the user as a string.</param>
/// <param name="messageContent">Content of the error message as a string.</param>
/// <param name="messageID">ID of the message as a byte. Unused in the TCP variant, hence the default value.</param>
public class ErrMessage(string displayName, string messageContent, ushort messageID = 0) : Message(MessageType.ERR)
{
    /// <summary>
    /// Regular expression for the textual version of the ERR message.
    /// </summary>
    public const string Format = @$"^ERR FROM (?<DISPLAY_NAME>{ParameterFormats.DISPLAY_NAME}) IS (?<MESSAGE_CONTENT>{ParameterFormats.MESSAGE_CONTENT})$";

    /// <summary>
    /// ID of the message.
    /// </summary>
    private ushort MessageID = messageID;

    /// <summary>
    /// Error message.
    /// </summary>
    public readonly string DisplayName = displayName;

    /// <summary>
    /// Content of the error message.
    /// </summary>
    public readonly string MessageContent = messageContent;

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
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes(ushort messageID)
    {
        // Set the message ID
        MessageID = messageID;

        // Message fields
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(MessageID));
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
        byte[] messageContentBytes = Encoding.ASCII.GetBytes(MessageContent);

        // Serialize into a byte array
        return [(byte)MessageType.ERR, .. messageIDBytes, .. displayNameBytes, 0, .. messageContentBytes, 0];
    }

    /// <summary>
    /// Tries to parse an ERR message from a byte array.
    /// </summary>
    /// <param name="response">Byte array representing the message.</param>
    /// <param name="message">The resulting ERR message if parsed succesfully, else null.</param>
    public static bool TryParse(byte[] response, out ErrMessage? message)
    {
        // Has to be at least |0xFE|MessageID|MessageID|DisplayName|0|MessageContent|0|
        if(response.Length < 7)
        {
            message = null;
            return false;
        }

        // Message ID
        ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(response, 1));

        // Display name
        int messageContentStart = ParseDisplayName(response, 3, out bool success, out string? displayName);
        if(!success)
        {
            message = null;
            return false;
        }

        // Message content
        string? messageContent = ParseMessageContent(response, messageContentStart, out success);
        if(!success)
        {
            message = null;
            return false;
        }

        // Create and return the message
        message = new ErrMessage(displayName!, messageContent!, messageID);
        return true;
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"ERR FROM {DisplayName} IS {MessageContent}\r\n";

    /// <summary>
    /// Returns the message ID. Needed for compatibility with the Message class.
    /// </summary>
    /// <returns>Message ID.</returns>
    public override ushort GetMessageID() => MessageID;
}