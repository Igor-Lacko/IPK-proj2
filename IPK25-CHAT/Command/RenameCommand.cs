/* Contains the RENAME command class */

namespace IPK_25_CHAT.Command;

using IPK_25_CHAT.Enum;

class RenameCommand(string displayName) : Command(CommandType.RENAME, $"/rename {displayName}")
{
    /// <summary>
    /// Regular expression for the RENAME command.
    /// </summary>
    public const string Format = @$"^/rename\s+{ParameterFormats.DISPLAY_NAME}$";

    /// <summary>
    /// User's new displayed name.
    /// </summary>
    private readonly string DisplayName = displayName;

    /// <summary>
    /// Validates the rename command. Is valid if the user has a set display name.
    /// (E.g. in any state after AUTH, so not AUTH or START)
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the command is valid, else false.</returns>
    public override bool IsValid(State clientState) => !(clientState == State.AUTH || clientState == State.START);
}