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
public class MsgMessage(string displayName, string messageContent) : Message(MessageType.MSG)
{
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
    public override byte[] AsBytes(short messageID)
    {
        // Message fields
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageID));
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
        byte[] messageContentBytes = Encoding.ASCII.GetBytes(MessageContent);
        
        // Serialize into a byte array
        return [(byte)MessageType.MSG, .. messageIDBytes, .. displayNameBytes, 0, .. messageContentBytes, 0];
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"MSG FROM {DisplayName} IS {MessageContent}\r\n";
}