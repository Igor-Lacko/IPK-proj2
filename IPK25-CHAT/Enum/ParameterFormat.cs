/* Contains an enumeration of command/message parameters (as regular expressions) */

namespace IPK_25_CHAT.Enum;

/// <summary>
/// Enumeration of command/message parameters.
/// Is not a enum, since enums can't have a string base class, but a static class with constant fields as a workaround.
/// </summary>
public static class ParameterFormats
{
    /// <summary>
    /// Regular expression for a username.
    /// </summary>
    public const string USERNAME = @"[a-zA-Z0-9_-]{1,20}";

    /// <summary>
    /// Regular expression for a password.
    /// </summary>
    public const string SECRET = @"[a-zA-Z0-9_-]{1,128}";

    /// <summary>
    /// Regular expression for a channel ID. The same as username.
    /// </summary>
    public const string CHANNEL_ID = @"[a-zA-Z0-9_-]{1,20}";

    /// <summary>
    /// Regular expression for the displayed name.
    /// </summary>
    public const string DISPLAY_NAME = @"[x21-x7e]{1,20}";

    /// <summary>
    /// Regular expression for message content.
    /// </summary>
    public const string MESSAGE_CONTENT = @"(x0a|[x20-x7e]){1,60000}";
}