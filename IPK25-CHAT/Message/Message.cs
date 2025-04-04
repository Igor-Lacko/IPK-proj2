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

        // All matches
        Match byeMatch = Regex.Match(message, ByeMessage.Format);
        Match errMatch = Regex.Match(message, ErrMessage.Format);
        Match msgMatch = Regex.Match(message, MsgMessage.Format);
        Match replyMatch = Regex.Match(message, ReplyMessage.Format);

        // Check individual message types

        // BYE FROM DNAME
        if(byeMatch.Success)
            return new ByeMessage(byeMatch.Groups["DISPLAY_NAME"].Value);

        // ERR FROM DNAME IS CONTENT
        else if(errMatch.Success)
            return new ErrMessage(errMatch.Groups["DISPLAY_NAME"].Value, errMatch.Groups["MESSAGE_CONTENT"].Value);

        // MSG FROM DNAME IS CONTENT
        else if(msgMatch.Success)
            return new MsgMessage(msgMatch.Groups["DISPLAY_NAME"].Value, msgMatch.Groups["MESSAGE_CONTENT"].Value);

        // REPLY (OK|NOK) IS CONTENT
        else if(replyMatch.Success)
        {
            bool ok = Regex.Split(message, @"\s+")[1] == "OK";
            return new ReplyMessage(ok, replyMatch.Groups["MESSAGE_CONTENT"].Value);
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