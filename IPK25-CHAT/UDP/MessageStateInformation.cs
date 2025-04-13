/* Class that contains the current state of a UDP message */

namespace IPK_25_CHAT.UDP;

/// <summary>
/// Class that contains the current state of a UDP message sent from the client.
/// </summary>
/// <param name="messageId">The message ID of the sent message.</param>
/// <param name="onConfirm">Task completion source for confirmation.</param>
public struct MessageStateInformation(ushort messageId, TaskCompletionSource<bool> onConfirm, bool isRequest)
{
    /// <summary>
    /// The message ID of the sent message.
    /// </summary>
    public ushort MessageID = messageId;

    /// <summary>
    /// Set to true if the message is a request (e.g. AUTH or JOIN).
    /// There can only be one message of this type at a time (well, there can only be one message of any type at a time since i wait for
    /// confirmation before sending another, but this approach seems safer)
    /// </summary>
    public bool IsRequest = isRequest;

    /// <summary>
    /// Set to true after confirmation is received.
    /// </summary>
    public TaskCompletionSource<bool> OnConfirm = onConfirm;

    /// <summary>
    /// Task that is completed when the message is confirmed.
    /// </summary>
    public Task<bool> MessageConfirmed = onConfirm.Task;
}