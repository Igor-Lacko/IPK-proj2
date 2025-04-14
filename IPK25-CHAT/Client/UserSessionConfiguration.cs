/* Has a class which contains user data */

namespace IPK_25_CHAT.Client;

/// <summary>
/// Class that contains data representing one client session.
/// </summary>
/// <param name="username">Username</param>
/// <param name="displayName">Current display name (default is "unknown")</param>
/// <param name="channelID">Channel that the user is currently in</param>
public class UserSessionConfiguration(string? username = null, string? displayName = null, string? channelID = null)
{
    /// <summary>
    /// Username.
    /// </summary>
    public string? Username = username;

    /// <summary>
    /// Name that is displayed to other users.
    /// </summary>
    public string DisplayName = displayName ?? "Unknown";

    /// <summary>
    /// ID of the chat channel.
    /// </summary>
    public string? ChannelID = channelID;

    /// <summary>
    /// ID of the channel that the user has requested to join.
    /// </summary>
    public string? RequestedChannelID = null;
}