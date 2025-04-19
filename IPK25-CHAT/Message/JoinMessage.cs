namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

using System.Net;
using System.Text;

/// <summary>
/// Class representing the JOIN message.
/// </summary>
/// <param name="displayName">Display name of the user as a string.</param>
/// <param name="channelID">ID of the chat channel to be joined as a string.</param>
public class JoinMessage(string displayName, string channelID) : Message(MessageType.JOIN)
{
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
    public override byte[] AsBytes(ushort messageID)
    {
        // Message fields
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)messageID));
        byte[] channelIDBytes = Encoding.ASCII.GetBytes(ChannelID);
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);

        // Serialize into a byte array
        return [(byte)MessageType.JOIN, .. messageIDBytes, .. channelIDBytes, 0, .. displayNameBytes, 0];
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"JOIN {ChannelID} AS {DisplayName}\r\n";

    /// <summary>
    /// Throws an exception, since JOIN messages can't be received froim the server.
    /// </summary>
    /// <exception cref="ArgumentException">Always thrown.</exception>
    public override ushort GetMessageID() => throw new ArgumentException("JOIN messages can't be received from the server.");
}