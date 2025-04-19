namespace IPK_25_CHAT.Enum;

/// <summary>
/// Enumeration of client states.
/// </summary>

public enum State
{
    /// <summary>
    /// Starting state.
    /// </summary>
    START,
    /// <summary>
    /// State where the client waits for authentication.
    /// </summary>
    AUTH,
    /// <summary>
    /// Client is in a chat channel.
    /// </summary>
    OPEN,
    /// <summary>
    /// Client is trying to join a chat channel.
    /// </summary>
    JOIN,
    /// <summary>
    /// Final state.
    /// </summary>
    END
}