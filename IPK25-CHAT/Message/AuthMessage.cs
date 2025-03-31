/* Contains the AUTH message. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the AUTH message.
/// </summary>
public class AuthMessage : Message
{
    /// <summary>
    /// Username of the user.
    /// </summary>
    private readonly string Username;

    /// <summary>
    /// Password of the user.
    /// </summary>
    private readonly string Secret;

    /// <summary>
    /// User's displayed name.
    /// </summary>
    private readonly string DisplayName;

    /// <summary>
    /// Textual protocol constructor for the AUTH message.
    /// </summary>
    /// <param name="username">Username of the user as a string.</param>
    /// <param name="secret">Password of the user as a string.</param>
    /// <param name="displayName">User's displayed name as a string.</param>
    public AuthMessage(string username, string secret, string displayName) : base(MessageType.AUTH)
    {
        MessageID = null;
        Username = username;
        Secret = secret;
        DisplayName = displayName;
    }

    /// <summary>
    /// Binary protocol constructor for the AUTH message.
    /// </summary>
    /// <param name="MessageID">ID of the message as a ushort.</param>
    /// <param name="username">Username of the user as a byte array.</param>
    /// <param name="secret">Password of the user as a byte array.</param>
    public AuthMessage(ushort MessageID, byte[] username, byte[] secret, byte[] displayName) : base(MessageType.AUTH)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public override bool IsValid(State clientState)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts the message to a byte array.
    /// </summary>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
}