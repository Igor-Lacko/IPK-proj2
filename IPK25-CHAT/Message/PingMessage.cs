/* Contains the PING message. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the PING message, which is UDP specific.
/// </summary>
public class PingMessage : Message
{
    /// <summary>
    /// Constructor for the PING message.
    /// </summary>
    /// <param name="MessageID">Message ID as a ushort.</param>
    public PingMessage(ushort MessageID) : base(MessageType.PING)
    {
        this.MessageID = MessageID;
    }

    /// <summary>
    /// Checks if the message is valid in the current client state. Returns true always, since the server only sends this message.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True</returns>
    public override bool IsValid(State clientState) => true;

    /// <summary>
    /// Throws an exception, since this message should only be received, never sent.
    /// </summary>
    /// <returns>Throws an exception.</returns>
    /// <exception cref="ArgumentException">Thrown when the method is called.</exception>
    public override byte[] AsBytes() => throw new ArgumentException("Client does not allow you to send PING messages!");

    /// <summary>
    /// Throws an exception, since this is a purely binary message.
    /// </summary>
    /// <returns>Throws an exception.</returns>
    /// <exception cref="ArgumentException">Thrown when the method is called.</exception>
    public override string AsString() => throw new ArgumentException("PING message is not used in textual form.");
}