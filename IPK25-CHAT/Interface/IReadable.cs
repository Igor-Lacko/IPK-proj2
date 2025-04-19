namespace IPK_25_CHAT.Interface;

using IPK_25_CHAT.Enum;

/// <summary>
/// Interface for readable inputs (messages/commands) from either the server or user.
/// </summary>
public interface IReadable
{
    /// <summary>
    /// Checks if the input is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the command/message is valid, else false.</returns>
    public bool IsValid(State clientState);
}