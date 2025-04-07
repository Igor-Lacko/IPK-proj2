/* Contains the JOIN message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the JOIN message.
/// </summary>
public class JoinMessage : Message
{
    /// <summary>
    /// Display name of the user.
    /// </summary>
    private readonly string DisplayName;

    /// <summary>
    /// Id of the chat channel to be joined.
    /// </summary>
    private readonly string ChannelID;

    /// <summary>
    /// Textual protocol constructor for JOIN message.
    /// </summary>
    /// <param name="displayName">Display name of the user as a string.</param>
    /// <param name="channelID">ID of the chat channel to be joined as a string.</param>
    public JoinMessage(string displayName, string channelID) : base(MessageType.JOIN)
    {
        DisplayName = displayName;
        ChannelID = channelID;
    }

    /// <summary>
    /// Binary protocol constructor for JOIN message.
    /// </summary>
    /// <param name="messageID">ID of the message as a ushort.</param>
    /// <param name="displayName">Display name of the user as a byte array.</param>
    /// <param name="channelID">ID of the chat channel to be joined as a byte array.</param>
    public JoinMessage(ushort MessageID, byte[] displayName, byte[] channelID) : base(MessageType.JOIN)
    {
        throw new NotImplementedException();
    }

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
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes()
    {
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)MessageID!));
        byte[] channelIDBytes = Encoding.ASCII.GetBytes(ChannelID);
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
        return [(byte)MessageType.JOIN, .. messageIDBytes, .. channelIDBytes, 0, .. displayNameBytes, 0];
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"JOIN {ChannelID} AS {DisplayName}\r\n";
}