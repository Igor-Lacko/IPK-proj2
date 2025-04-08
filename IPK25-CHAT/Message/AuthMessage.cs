/* Contains the AUTH message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
using IPK_25_CHAT.Command;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the AUTH message.
/// </summary>
/// <param name="command">Command containing the username, password, and display name.</param>
public class AuthMessage(AuthCommand command) : Message(MessageType.AUTH)
{
    /// <summary>
    /// Username of the user.
    /// </summary>
    private readonly string Username = command.Username;

    /// <summary>
    /// Password of the user.
    /// </summary>
    private readonly string Secret = command.Secret;

    /// <summary>
    /// User's displayed name.
    /// </summary>
    private readonly string DisplayName = command.DisplayName;

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public override bool IsValid(State clientState) => clientState == State.START || clientState == State.AUTH;

    /// <summary>
    /// Converts the message to a byte array.
    /// |0x02|MESSAGEID|MESSAGEID|USERNAME....|0|DISPLAYNAME....|0|SECRET....|0|
    /// </summary>
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes(short messageID)
    {
        // Message fields
        byte[] messageIDBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageID));
        byte[] usernameBytes = Encoding.ASCII.GetBytes(Username);
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(DisplayName);
        byte[] secretBytes = Encoding.ASCII.GetBytes(Secret);

        // Serialize into a byte array
        return [(byte)MessageType.AUTH, .. messageIDBytes, .. usernameBytes, 0, .. displayNameBytes, 0, .. secretBytes, 0];
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
}