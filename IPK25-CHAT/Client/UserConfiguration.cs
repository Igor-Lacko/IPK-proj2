/* Has a structure which contains user data */

namespace IPK_25_CHAT.Client;

/// <summary>
/// Structure that contains data representing one client session.
/// </summary>
public struct UserSessionConfiguration(string? username = null, string? displayName = null, string? channelID = null)
{
    /// <summary>
    /// Username.
    /// </summary>
    public string? Username = username;

    /// <summary>
    /// Name that is displayed to other users.
    /// </summary>
    public string? DisplayName = displayName;

    /// <summary>
    /// ID of the chat channel.
    /// </summary>
    public string? ChannelID = channelID;
}