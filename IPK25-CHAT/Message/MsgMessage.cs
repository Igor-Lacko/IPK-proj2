/* Contains the MSG message. */

namespace IPK_25_CHAT.Message;

using IPK_25_CHAT.Enum;

/// <summary>
/// Class representing the MSG message.
/// </summary>
public class MsgMessage : Message
{
    /// <summary>
    /// Regular expression for the textual version of the MSG message.
    /// </summary>
    public const string Format = @$"^MSG FROM (?<DISPLAY_NAME>{ParameterFormats.DISPLAY_NAME}) IS (?<MESSAGE_CONTENT>{ParameterFormats.MESSAGE_CONTENT})$";

    /// <summary>
    /// Display name of the user.
    /// </summary>
    public readonly string DisplayName;

    /// <summary>
    /// Content of the message.
    /// </summary>
    public readonly string MessageContent;

    /// <summary>
    /// Textual protocol constructor for MSG message.
    /// </summary>
    /// <param name="displayName">Display name of the user as a string.</param>
    /// <param name="messageContent">Content of the message as a string.</param>
    public MsgMessage(string displayName, string messageContent) : base(MessageType.MSG)
    {
        DisplayName = displayName;
        MessageContent = messageContent;
    }

    /// <summary>
    /// Binary protocol constructor for MSG message.
    /// </summary>
    /// <param name="messageID">ID of the message as a ushort.</param>
    /// <param name="displayName">Display name of the user as a byte array.</param>
    /// <param name="messageContent">Content of the message as a byte array.</param>
    public MsgMessage(ushort MessageID, byte[] displayName, byte[] messageContent) : base(MessageType.MSG)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the message is valid in the current client state.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True if the message is valid, else false.</returns>
    public override bool IsValid(State clientState) => clientState == State.OPEN || clientState == State.JOIN;

    /// <summary>
    /// Converts the message to a byte array.
    /// </summary>
    /// <returns>Byte array representing the message.</returns>
    public override byte[] AsBytes()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>String representing the message.</returns>
    public override string ToString() => $"MSG FROM {DisplayName} IS {MessageContent}\r\n";
}