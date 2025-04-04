/* Contains the enumeration of command types*/

namespace IPK_25_CHAT.Enum;

/// <summary>
/// Enumeration of command types.
/// </summary>
/// <remarks>
/// Maybe i should add more later?
/// </remarks>
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