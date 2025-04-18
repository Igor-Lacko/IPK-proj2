/* Contains the base class for a message */

namespace IPK_25_CHAT.Message;

using System.Net;
using System.Text;
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
            return new MalformedMessage(null);

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
            bool ok = Regex.Split(message, @"\s+")[1].ToUpper() == "OK";
            return new ReplyMessage(ok, replyMatch.Groups["MESSAGE_CONTENT"].Value);
        }

        // No match found
        return new MalformedMessage(null);
    }

    /// <summary>
    /// Parses the message byte array into a message object.
    /// </summary>
    /// <param name="message">Byte array representing the message. Is UDP only.</param>
    /// <param name="bytesReceived">Number of bytes received.</param>
    /// <returns>True if parsed successfully, else it returns false.</returns>
    public static Message Parse(byte[] message, int bytesReceived)
    {
        // From the message type
        switch(message[0])
        {
            case (byte)MessageType.BYE:
                if(ByeMessage.TryParse(message, bytesReceived, out ByeMessage? byeMessage))
                    return byeMessage!;

                break;

            case (byte)MessageType.ERR:
                if(ErrMessage.TryParse(message, bytesReceived, out ErrMessage? errMessage))
                    return errMessage!;

                break;

            case (byte)MessageType.CONFIRM:
                if(ConfirmMessage.TryParse(message, bytesReceived, out ConfirmMessage? confirmMessage))
                    return confirmMessage!;

                break;

            case(byte)MessageType.PING:
                if(PingMessage.TryParse(message, bytesReceived, out PingMessage? pingMessage))
                    return pingMessage!;

                break;

            case (byte)MessageType.REPLY:
                if(ReplyMessage.TryParse(message, bytesReceived, out ReplyMessage? replyMessage))
                    return replyMessage!;

                break;

            case (byte)MessageType.MSG:
                if(MsgMessage.TryParse(message, bytesReceived, out MsgMessage? msgMessage))
                    return msgMessage!;

                break;

            // No match for message type
            default:
                // Try to pick up the message ID
                if(bytesReceived >= 3)
                {
                    ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(message, 1));
                    return new MalformedMessage(messageID);
                }

                else return new MalformedMessage(null);
        }

        // Matched message type but failed to parse
        // Again, try to pick up the message ID
        if(bytesReceived >= 3)
        {
            ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(message, 1));
            return new MalformedMessage(messageID);
        }

        else return new MalformedMessage(null);
    }

    /// <summary>
    /// Parses the display name from a byte array.
    /// </summary>
    /// <param name="message">Array containing the display name.</param>
    /// <param name="startIndex">The start of the index in the array where we are supposed to look for the display name.</param>
    /// <param name="displayName">The display name if parsed successfully, else null.</param>
    /// <returns>The index AFTER the zero which terminates the display name.</returns>
    protected static int ParseDisplayName(byte[] message, int startIndex, out string? displayName)
    {
        // Help variables
        List<byte> displayNameBytes = [];
        uint count = 0;

        // Set at start
        displayName = null;

        // Loop through the section of the array starting at startIndex
        foreach(byte displayNameByte in message[startIndex..])
        {
            // Message end
            if(displayNameByte == 0) break;

            // Display name too long
            else if(count++ > 20)
                return 0;

            // Not a printable character
            else if(displayNameByte < 0x21 || displayNameByte > 0x7E)
                return 0;

            // Add to the display name
            else displayNameBytes.Add(displayNameByte);
        }

        // Return the display name converted to a string
        displayName = Encoding.ASCII.GetString([.. displayNameBytes]);

        // Start + count + trailing zero
        return startIndex + displayName.Length + 1;
    }

    /// <summary>
    /// Parses the channel ID from a byte array.
    /// </summary>
    /// <param name="message">The byte array to search for the channel ID.</param>
    /// <param name="startIndex">Start of the array section to look for the channel ID.</param>
    /// <param name="channelID">Contains the outgoing string representation of the channel iD if parsed successfully, else null.</param>
    /// <returns>Index after the trailing zero after the channelID.</returns>
    protected static int ParseChannelID(byte[] message, int startIndex, out string? channelID)
    {
        // Help variables
        List<byte> channelIDBytes = [];
        uint count = 0;

        // Set at start
        channelID = null;

        // Loop through the section of the array starting at startIndex
        foreach(byte channelIDByte in message[startIndex..])
        {
            // Message end
            if(channelIDByte == 0) break;

            // Channel ID too long
            else if(count++ > 20)
                return 0;

            // Not a alphanumeric character or one of -,_
            else if(!(channelIDByte >= 'a' && channelIDByte <= 'z') &&
                    !(channelIDByte >= 'A' && channelIDByte <= 'Z') &&
                    !(channelIDByte >= '0' && channelIDByte <= '9') &&
                    channelIDByte != '-' && channelIDByte != '_')
                    return 0;

            // Valid character
            else channelIDBytes.Add(channelIDByte);
        }

        // Covert to string and return
        channelID = Encoding.ASCII.GetString([.. channelIDBytes]);
        return startIndex + channelID.Length + 1;
    }

    /// <summary>
    /// Parses the message content parameter from a byte array.
    /// Does not need to return the index after the message content since it's always at the tail of the message.
    /// </summary>
    /// <param name="message">The byte array where to search for the message content.</param>
    /// <param name="startIndex">Start of the array section to look for the message content.</param>
    /// <param name="messageContent">The message content if parsed successfully, else null.</param>
    /// <returns>The index after the message content AND the trailing zero (to check for trailing bytes)</returns>
    protected static int ParseMessageContent(byte[] message, int startIndex, out string? messageContent)
    {
        // Help variables
        uint count = 0;
        List<byte> messageContentBytes = [];

        // Set at start
        messageContent = null;

        // Loop until zero byte
        foreach(byte messageContentByte in message[startIndex..])
        {
            // Message end
            if(messageContentByte == 0) break;

            // Message content too long
            else if(count++ > 60000)
                return 0;

            // Not a printable character,space or a line feed
            else if(messageContentByte != 0x0A && (messageContentByte < 0x20 || messageContentByte > 0x7E))
                return 0;

            // Valid
            else messageContentBytes.Add(messageContentByte);
        }

        // Convert to string, return
        messageContent = Encoding.ASCII.GetString([.. messageContentBytes]);
        return startIndex + messageContent.Length + 1;
    }

    /// <summary>
    /// Converts the message to a byte array.
    /// </summary>
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Byte array representing the message.</returns>
    public abstract byte[] AsBytes(ushort messageID);

    /// <summary>
    /// Returns the message's ID. Only used in some message types
    /// </summary>
    public abstract ushort GetMessageID();
}