/* Class that contains the current state of a UDP message */

namespace IPK_25_CHAT.UDP;

/// <summary>
/// Class that contains the current state of a UDP message sent from the client.
/// </summary>
/// <param name="messageId">The message ID of the sent message.</param>
/// <param name="onConfirm">Task completion source for confirmation.</param>
public struct MessageStateInformation(ushort messageId, TaskCompletionSource<bool> onConfirm)
{
    /// <summary>
    /// The message ID of the sent message.
    /// </summary>
    public ushort MessageID = messageId;

    /// <summary>
    /// Set to true after confirmation is received.
    /// </summary>
    public TaskCompletionSource<bool> OnConfirm = onConfirm;

    /// <summary>
    /// Task that is completed when the message is confirmed.
    /// </summary>
    public Task<bool> MessageConfirmed = onConfirm.Task;
}