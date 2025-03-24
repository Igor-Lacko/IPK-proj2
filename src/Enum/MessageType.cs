/* Contains an enumeration of message types. */

namespace src.Enum;

/// <summary>
/// Enumeration of message types.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Client authentication.
    /// </summary>
    AUTH,

    /// <summary>
    /// Terminate conversation.
    /// </summary>
    BYE,

    /// <summary>
    /// UDP only. Successful message delivery.
    /// </summary>
    CONFIRM,

    /// <summary>
    /// Error while processing the other party's last message.
    /// </summary>
    ERR,

    /// <summary>
    /// Client's request to join a chat channel.
    /// </summary>
    JOIN,

    /// <summary>
    /// Message.
    /// </summary>
    MSG,

    /// <summary>
    /// UDP only. Check if the other party is "still alive".
    /// </summary>
    PING,

    /// <summary>
    /// Server's response to a client's request.
    /// </summary>
    REPLY
}