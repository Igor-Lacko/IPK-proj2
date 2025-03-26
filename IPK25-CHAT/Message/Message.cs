/* Contains the base class for a message */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;
using IPK_25_CHAT.Interface;

/// <summary>
/// Base class for a message.
/// </summary>
/// <param name="type">Message type.</param>
public abstract class Message(MessageType type) : IReadable
{
    /// <summary>
    /// Message type.
    /// </summary>
    public readonly MessageType Type = type;

    /// <summary>
    /// ID of the message. UDP specific, so it is nullable.
    /// </summary>
    protected ushort? MessageID;

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public abstract bool IsValid(State clientState);

    /// <summary>
    /// Parses the message string into a message object.
    /// </summary>
    /// <param name="message">Message string.</param>
    /// <param name="result">Variable to store the result, if parsed successfully.</param>
    /// <returns>True if parsed successfully, else it returns False.</returns>
    public static bool Parse(string message, out Message result)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parses the message byte array into a message object.
    /// </summary>
    /// <param name="message">Byte array representing the message. Is UDP only.</param>
    /// <param name="result">Variable to store the result, if parsed successfully.</param>
    /// <returns>True if parsed successfully, else it returns false.</returns>
    public static bool Parse(byte[] message, out Message result)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts the message to a byte array.
    /// </summary>
    /// <returns>Byte array representing the message.</returns>
    public abstract byte[] AsBytes();

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public abstract string AsString();
}