/* Contains the AUTH command class */

namespace IPK_25_CHAT.Command;
using IPK_25_CHAT.Enum;

/// <summary>
/// AUTH command class.
/// </summary>
public class AuthCommand(string username, string secret, string displayName) : Command(CommandType.AUTH, $"/auth {username} {secret} {displayName}")
{
    /// <summary>
    /// Regular expression for the AUTH command.
    /// </summary>
    public const string Format = @$"^/auth\s+{ParameterFormats.USERNAME}\s+{ParameterFormats.SECRET}\s+{ParameterFormats.DISPLAY_NAME}$"; // todo

    /// <summary>
    /// Username of the, well, user.
    /// </summary>
    public readonly string Username = username;

    /// <summary>
    /// Used for authentication.
    /// </summary>
    public readonly string Secret = secret;

    /// <summary>
    /// Display name of the user.
    /// </summary>
    public readonly string DisplayName = displayName;

    /// <summary>
    /// Validates the auth command. Is only valid in the START state. (TODO? Might also AUTH)
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the command is valid, else false.</returns>
    public override bool IsValid(State clientState) => clientState == State.START || clientState == State.AUTH;
}