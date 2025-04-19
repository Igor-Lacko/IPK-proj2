namespace IPK_25_CHAT.Command;

using IPK_25_CHAT.Enum;

public class RenameCommand(string displayName) : Command(CommandType.RENAME)
{
    /// <summary>
    /// Regular expression for the RENAME command.
    /// </summary>
    public const string Format = @$"^/rename\s+{ParameterFormats.DISPLAY_NAME}$";

    /// <summary>
    /// User's new displayed name.
    /// </summary>
    public readonly string DisplayName = displayName;

    /// <summary>
    /// Is always valid. Is a local command, the user can always set their name locallly.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True.</returns>
    public override bool IsValid(State clientState) => true;
}