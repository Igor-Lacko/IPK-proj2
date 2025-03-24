/* Contains the base class for a message */

namespace src.Client;

using src.Enum;
using src.Interface;

/// <summary>
/// Base class for a message.
/// </summary>
/// <param name="type">Message type.</param>
/// <param name="message">Message as a string.</param>
public abstract class Message(MessageType type, string message) : IReadable
{
    /// <summary>
    /// Message type.
    /// </summary>
    public MessageType Type { get; } = type;

    /// <summary>
    /// Message as a string.
    /// </summary>
    protected string MessageAsString { get; } = message;

    /// <summary>
    /// Returns the string representation of the message.
    /// </summary>
    /// <returns>String representation of the message.</returns>
    public string AsString() => MessageAsString;

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public abstract bool IsValid(State clientState);
}