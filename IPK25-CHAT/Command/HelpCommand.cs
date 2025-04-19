namespace IPK_25_CHAT.Command;

using IPK_25_CHAT.Enum;

/// <summary>
/// HELP command class.
/// </summary>
public class HelpCommand() : Command(CommandType.HELP)
{
    /// <summary>
    /// Regular expression for the HELP command.
    /// </summary>
    public const string Format = @"^/help$";

    /// <summary>
    /// Returns true (the user can always ask for the help command).
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True.</returns>
    public override bool IsValid(State clientState) => true;
}