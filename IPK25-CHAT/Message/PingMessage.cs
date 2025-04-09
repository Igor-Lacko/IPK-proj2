/* Contains the PING message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the PING message, which is UDP specific.
/// </summary>
/// <param name="messageID">ID of the message.</param>
public class PingMessage(ushort messageID) : Message(MessageType.PING)
{
    /// <summary>
    /// ID of the message.
    /// </summary>
    private ushort MessageID = messageID;

    /// <summary>
    /// Checks if the message is valid in the current client state. Returns true always, since the server only sends this message.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True</returns>
    public override bool IsValid(State clientState) => true;

    /// <summary>
    /// Throws an exception, since this message should only be received, never sent.
    /// </summary>
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Throws an exception.</returns>
    /// <exception cref="ArgumentException">Thrown when the method is called.</exception>
    public override byte[] AsBytes(ushort messageID) => throw new ArgumentException("Client does not allow you to send PING messages!");

    /// <summary>
    /// Tries to parse a PING message from a byte array.
    /// </summary>
    /// <param name="response">Byte array representing the message.</param>
    /// <param name="message">The resulting PING message if parsed successfully, else null.</param>
    /// <returns>True if parsed successfully, else false.</returns>
    public static bool TryParse(byte[] response, out PingMessage? message)
    {
        // Has to be |0x01|MessageID|MessageID|
        if (response.Length != 3)
        {
            message = null;
            return false;
        }

        // Message ID
        ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(response, 1));
        message = new PingMessage(messageID);
        return true;
    }

    /// <summary>
    /// Throws an exception, since this is a purely binary message.
    /// </summary>
    /// <returns>Throws an exception.</returns>
    /// <exception cref="ArgumentException">Thrown when the method is called.</exception>
    public override string ToString() => throw new ArgumentException("PING message is not used in textual form.");

    /// <summary>
    /// Returns the message ID.
    /// </summary>
    /// <returns>Message ID.</returns>
    public override ushort GetMessageID() => MessageID;
}