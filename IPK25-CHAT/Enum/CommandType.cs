namespace IPK_25_CHAT.Enum;

/// <summary>
/// Enumeration of command types.
/// </summary>
public enum CommandType
{
    /// <summary>
    /// Connect to the server.
    /// </sumamry>
    AUTH,

    /// <summary>
    /// Connect to a chat channel.
    /// </summary>
    JOIN,

    /// <summary>
    /// Rename the user.
    /// </summary>
    RENAME,

    /// <summary>
    /// Show all suppoorted commands.
    /// </summary>
    HELP,

    /// <summary>
    /// Show the user status.
    /// </summary>
    STATUS,
}