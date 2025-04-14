/* Contains the CONFIRM message. */

namespace IPK_25_CHAT.Message;

using System.Net;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the CONFIRM message. This message is UDP specific, and as such is only used in binary form.
/// </summary>
/// <param name="messageID">ID of the message that was being comfirmed.</param>
public class ConfirmMessage(ushort messageID) : Message(MessageType.CONFIRM)
{
    /// <summary>
    /// ID of the message that was being comfirmed.
    /// </summary>
    private ushort RefMessageID = messageID;

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// Might not even be used for this mesasge type..?
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public override bool IsValid(State clientState) => true;

    /// <summary>
    /// Converts the message to a byte array.
    /// </summary>
    /// <param name="messageID">ID of the message.</param>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes(ushort messageID)
    {
        RefMessageID = messageID;
        return [(byte)MessageType.CONFIRM, .. BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)RefMessageID))];
    }

    /// <summary>
    /// Tries to parse a CONFIRM message from a byte array.
    /// </summary>
    /// <param name="response">The response received.</param>
    /// <param name="bytesReceived">Number of bytes received.</param>
    /// <param name="message">If parsed succesfully, this contains the outgoing CONFIRM message. Else null.</param>
    /// <returns>True if parsed succesfully, else false.</returns>
    public static bool TryParse(byte[] response, int bytesReceived, out ConfirmMessage? message)
    {
        // Has to be exactly |0x00|RefMessageID|RefMessageID|
        if(bytesReceived != 3)
        {
            message = null;
            return false;
        }

        ushort messageID = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(response, 1));
        message = new ConfirmMessage(messageID);
        return true;
    }

    /// <summary>
    /// Returns the message ID of the referenced message.
    /// </summary>
    /// <returns>Message ID of the referenced message.</returns>
    public override ushort GetMessageID() => RefMessageID;
}