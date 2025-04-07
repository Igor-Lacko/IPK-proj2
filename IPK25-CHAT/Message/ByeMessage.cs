/* Contains the BYE message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the BYE message.
/// </summary>
public class ByeMessage : Message
{
    /// <summary>
    /// Regular expression for the textual version of the BYE message.
    /// </summary>
    public const string Format = @$"^BYE FROM (?<DISPLAY_NAME>{ParameterFormats.DISPLAY_NAME})$";

    /// <summary>
    /// Display name of the user.
    /// </summary>
    private readonly string DisplayName;

    /// <summary>
    /// Textual protocol constructor for BYE message.
    /// </summary>
    /// <param name="displayName">Display name of the user as a string.</param>
    public ByeMessage(string displayName) : base(MessageType.BYE)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// Binary protocol constructor for BYE message.
    /// </summary>
    /// <param name="messageID">ID of the message as a ushort.</param>
    /// <param name="displayName">Display name of the user as a byte array.</param>
    public ByeMessage(ushort MessageID, byte[] displayName) : base(MessageType.BYE)
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
    /// </summary>
    /// |0xFF|MessageID|DisplayName|0|
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes()
    {
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)MessageID!));
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
        return [(byte)MessageType.BYE, .. messageIDBytes, .. displayNameBytes, 0];
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"BYE FROM {DisplayName}\r\n";
}