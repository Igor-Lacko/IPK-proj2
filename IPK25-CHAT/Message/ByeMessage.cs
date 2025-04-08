/* Contains the BYE message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the BYE message.
/// </summary>
/// <param name="displayName">Display name of the user as a string.</param>
/// <param name="messageID">ID of the message as a ushort. Unused in the TCP variant, hece the default value.</param>
public class ByeMessage(string displayName, ushort messageID = 0) : Message(MessageType.BYE)
{
    /// <summary>
    /// Regular expression for the textual version of the BYE message.
    /// </summary>
    public const string Format = @$"^BYE FROM (?<DISPLAY_NAME>{ParameterFormats.DISPLAY_NAME})$";

    /// <summary>
    /// Display name of the user.
    /// </summary>
    private readonly string DisplayName = displayName;

    /// <summary>
    /// ID of the message.
    /// </summary>
    public readonly ushort MessageID = messageID;

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public override bool IsValid(State clientState) => true;

    /// <summary>
    /// Converts the message to a byte array.
    /// |0xFF|MessageID|DisplayName|0|
    /// </summary>
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes(short messageID)
    {
        // Message fields
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageID!));
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);

        // Serialize into a byte array
        return [(byte)MessageType.BYE, .. messageIDBytes, .. displayNameBytes, 0];
    }

    /// <summary>
    /// Tries to parse a BYE message from a byte array.
    /// </summary>
    public static bool TryParse(byte[] response, out ByeMessage? message)
    {
        // At least |0xFF|MessageID|MessageID|DisplayName|0|
        if(response.Length < 5)
        {
            message = null;
            return false;
        }

        // Message ID
        ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 1));

        // Try to parse the display name
        _ = ParseDisplayName(response, 3, out bool success, out string? displayName);

        if(!success)
        {
            message = null;
            return false;
        }

        // Create and return the message
        message = new ByeMessage(displayName!, messageID);
        return true;
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"BYE FROM {DisplayName}\r\n";
}