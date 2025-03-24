/* Common interface for readable inputs from either the server (messages) or user (messages/commands) */

namespace src.Interface;

using src.Enum;

/// <summary>
/// Interface for readable inputs (messages/commands) from either the server or user.
/// </summary>
public interface IReadable
{
    /// <summary>
    /// Converts the input to a string.
    /// </summary>
    /// <returns>String representation of the input.</returns>
    public string AsString();

    /// <summary>
    /// Checks if the input is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the command/message is valid, else false.</returns>
    public bool IsValid(State clientState);
}