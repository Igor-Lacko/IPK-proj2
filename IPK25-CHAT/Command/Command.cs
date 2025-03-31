/* Contains the base class for a client command */

namespace IPK_25_CHAT.Command;

using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Enum;
using System.Text.RegularExpressions;

/// <summary>
/// Base class for a client command.
/// </summary>
/// <param name="type">Command type.</param>
/// <param name="textualRepresentation">Command as a string (for errors).</param>
public abstract class Command(CommandType type, string textualRepresentation) : IReadable
{
    /// <summary>
    /// Command type.
    /// </summary>
    public readonly CommandType Type = type;

    /// <summary>
    /// Command as a string (for errors).
    /// </summary>
    private readonly string TextualRepresentation = textualRepresentation;

    /// <summary>
    /// Override of ToString. Returns this.AsString.
    /// </summary>
    /// <returns>String representation of the command.</returns>
    public override string ToString() => TextualRepresentation;

    /// <summary>
    /// Checks if the command is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the command is valid, else false.</returns>
    public abstract bool IsValid(State clientState);

    /// <summary>
    /// Parses the command string into an command object.
    /// </summary>
    /// <param name="command"String to parse.</param>
    /// <param name="result">Variable to store the result if successful.</param>
    /// <returns>True if parsed successfully, else False.</returns>
    public static bool Parse(string command, out Command? result)
    {
        // Check for the prefix first
        if(!command.StartsWith('/'))
        {
            result = null;
            return false;
        }

        // Try to match the individual commands
        switch (command)
        {
            // /auth { Username } { Secret } { DisplayName }
            case var _ when Regex.IsMatch(command, AuthCommand.Format):
                string[] auth_split = Regex.Split(command, @"\s+");
                result = new AuthCommand(auth_split[1], auth_split[2], auth_split[3]);
                return true;

            // /rename { DisplayName }
            case var _ when Regex.IsMatch(command, RenameCommand.Format):
                string[] rename_split = Regex.Split(command, @"\s+");
                result = new RenameCommand(rename_split[1]);
                return true;

            // /join { ChannelID }
            case var _ when Regex.IsMatch(command, JoinCommand.Format):
                string[] join_split = Regex.Split(command, @"\s+");
                result = new JoinCommand(join_split[1]);
                return true;

            // /help
            case var _ when Regex.IsMatch(command, HelpCommand.Format):
                result = new HelpCommand();
                return true;

            // Default case
            default:
                result = null;
                return false;
        }
    }
}