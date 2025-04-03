/* Contains an enumeration of message types. */

namespace IPK_25_CHAT.Enum;

/// <summary>
/// Enumeration of message types.
/// </summary>
public enum MessageType : byte
{
    /// <summary>
    /// UDP only. Successful message delivery.
    /// </summary>
    CONFIRM = 0x00,

    /// <summary>
    /// Server's response to a client's request.
    /// </summary>
    REPLY = 0x01,

    /// <summary>
    /// Client authentication.
    /// </summary>
    AUTH = 0x02,

    /// <summary>
    /// Client's request to join a chat channel.
    /// </summary>
    JOIN = 0x03,

    /// <summary>
    /// Message.
    /// </summary>
    MSG = 0x04,

    /// <summary>
    /// Malformed message. Not a valid mesage, created after  a invalid message is received from the server.
    /// </summary>
    MALFORMED = 0x05,

    /// <summary>
    /// UDP only. Check if the other party is "still alive".
    /// </summary>
    PING = 0xFD,

    /// <summary>
    /// Error while processing the other party's last message.
    /// </summary>
    ERR = 0xFE,

    /// <summary>
    /// Terminate conversation.
    /// </summary>
    BYE = 0xFF
}