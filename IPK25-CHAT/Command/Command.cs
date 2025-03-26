/* Contains the base class for a client command */

namespace IPK_25_CHAT.Command;

using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Enum;

/// <summary>
/// Base class for a client command.
/// </summary>
/// <param name="type">Command type.</param>
public abstract class Command(CommandType type) : IReadable
{
    /// <summary>
    /// Command type.
    /// </summary>
    public CommandType Type { get; } = type;

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
    public static bool Parse(string command, out Command result)
    {
        throw new NotImplementedException();
    }
}