namespace IPK_25_CHAT.Enum;

/// <summary>
/// Exit codes for the client.
/// </summary>
public enum ExitCodes : int
{
    /// <summary>
    /// Normal run of the client, without problems.
    /// </summary>
    SUCCESS = 0,

    /// <summary>
    /// An ERR message is received from the server.
    /// </summary>
    ERR_RECEIVED = 10,

    /// <summary>
    /// Malformed message received from the server.
    /// </summary>
    MALFORMED_MESSAGE_RECEIVED = 20,

    /// <summary>
    /// UDP message was not confirmed in time.
    /// </summary>
    UDP_CONFIRM_TIMEOUT = 30,

    /// <summary>
    /// A reply was not received in time to a AUTH/JOIN.
    /// </summary>
    REPLY_TIMEOUT = 40,

    /// <summary>
    /// Invalid message for the given state.
    /// </summary>
    INVALID_MESSAGE = 50,

    /// <summary>
    /// All other errors.
    /// </summary>
    ERROR_OTHER = 60
}