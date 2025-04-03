/* Contains the base class for a message */

namespace IPK_25_CHAT.Message;

using System.Text.RegularExpressions;
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
    /// <returns>True if parsed successfully, else it returns False.</returns>
    public static Message Parse(string? message)
    {
        // Mandatory null check
        if(message == null)
            return new MalformedMessage(message);

        // Check individual message types
        if(Regex.IsMatch(message, ByeMessage.Format))
        {
            string[] bye_split = Regex.Split(message, @"\s+");
            return new ByeMessage(bye_split[2]);
        }

        // ERR FROM DNAME IS CONTENT
        else if(Regex.IsMatch(message, ErrMessage.Format))
        {
            string[] err_split = Regex.Split(message, @"\s+");
            return new ErrMessage(err_split[2], err_split[4]);
        }

        // MSG FROM DNAME IS CONTENT
        else if(Regex.IsMatch(message, MsgMessage.Format))
        {
            string[] msg_split = Regex.Split(message, @"\s+");
            return new MsgMessage(msg_split[2], msg_split[4]);
        }

        // REPLY (OK|NOK) IS CONTENT
        else if(Regex.IsMatch(message, ReplyMessage.Format))
        {
            string[] reply_split = Regex.Split(message, @"\s+");
            return new ReplyMessage(reply_split[1] == "OK", reply_split[3]);
        }

        // No match found
        return new MalformedMessage(message);
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
}