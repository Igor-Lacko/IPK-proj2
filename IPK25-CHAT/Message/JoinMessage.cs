/* Contains the JOIN message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the JOIN message.
/// </summary>
/// <param name="displayName">Display name of the user as a string.</param>
/// <param name="channelID">ID of the chat channel to be joined as a string.</param>
/// <param name="messageID">ID of the message. UDP specific, hence the default value</param>
public class JoinMessage(string displayName, string channelID, ushort messageID = 0) : Message(MessageType.JOIN)
{
    /// <summary>
    /// Message ID.
    /// </summary>
    public readonly ushort MessageID = messageID;

    /// <summary>
    /// Display name of the user.
    /// </summary>
    private readonly string DisplayName = displayName;

    /// <summary>
    /// Id of the chat channel to be joined.
    /// </summary>
    private readonly string ChannelID = channelID;

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public override bool IsValid(State clientState) => clientState == State.OPEN;

    /// <summary>
    /// Converts the message to a byte array.
    /// |0x03|MessageID|ChannelID|0|DisplayName|0|
    /// </summary>
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes(short messageID)
    {
        // Message fields
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageID));
        byte[] channelIDBytes = Encoding.ASCII.GetBytes(ChannelID);
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);

        // Serialize into a byte array
        return [(byte)MessageType.JOIN, .. messageIDBytes, .. channelIDBytes, 0, .. displayNameBytes, 0];
    }

    /// <summary>
    /// Tries to parse a JOIN message from a byte array.
    /// </summary>
    /// <param name="response">Byte array representing the message.</param>
    /// <param name="message">The resulting JOIN message if parsed succesfully, else null.</param>
    /// <returns>True if parsed succesfully, else false.</returns>
    public static bool TryParse(byte[] response, out JoinMessage? message)
    {
        // Has to be at least |0x03|MessageID|MessageID|ChannelID|0|DisplayName|0|
        if (response.Length < 7)
        {
            message = null;
            return false;
        }

        // Message ID
        ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(response, 1));

        // Channel ID
        int displayNameIndex = ParseChannelID(response, 3, out bool success, out string? channelID);
        if (!success)
        {
            message = null;
            return false;
        }

        // Display name
        _ = ParseDisplayName(response, displayNameIndex, out success, out string? displayName);
        if (!success)
        {
            message = null;
            return false;
        }

        // Create the message
        message = new JoinMessage(displayName!, channelID!, messageID);
        return true;
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"JOIN {ChannelID} AS {DisplayName}\r\n";
}