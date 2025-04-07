/* Has a class which contains user data */

namespace IPK_25_CHAT.Client;

/// <summary>
/// class that contains data representing one client session.
/// </summary>
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