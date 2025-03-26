/* Contains the AUTH message. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the AUTH message.
/// </summary>
/// <param name="username">Username of the, well, user.</param>
/// <param name="secret">Password of the user.</param>
/// <param name="displayName">User's displayed name.</param>
public class AuthMessage(string username, string secret, string displayName) : Message(MessageType.AUTH)
{
    /// <summary>
    /// Username of the user.
    /// </summary>
    public string Username { get; } = username;

    /// <summary>
    /// Password of the user.
    /// </summary>
    public string Secret { get; } = secret;

    /// <summary>
    /// User's displayed name.
    /// </summary>
    public string DisplayName { get; } = displayName;

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
    public override byte[] ToBytes()
    {
        throw new NotImplementedException();
    }
}