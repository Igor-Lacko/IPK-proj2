/* Static class for stdout prints. */

namespace IPK_25_CHAT.IO;

using IPK_25_CHAT.Message;

public static class StdoutResultWriter
{
    /// <summary>
    /// Prints a MSG message to stdout.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintMsgMessage(MsgMessage message)
    {
        Console.WriteLine($"{message.DisplayName}: {message.MessageContent}");
    }

    /// <summary>
    /// Prints an ERR message to stdout.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintErrMessage(ErrMessage message)
    {
        Console.WriteLine($"ERROR FROM {message.DisplayName}: {message.MessageContent}");
    }

    /// <summary>
    /// Prints a internal client error message to stdout.
    /// </summary>
    /// <param name="errorMessage">Error message to print.</param>
    public static void InternalClientError(string? errorMessage)
    {
        Console.WriteLine($"ERROR: {errorMessage}");
    }

    /// <summary>
    /// Prints a REPLY/!REPLY message to stdout.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintReplyMessage(ReplyMessage message)
    {
        if(message.OK) Console.WriteLine($"Action Success: {message.MessageContent}");
        else Console.WriteLine($"Action Failure: {message.MessageContent}");
    }

    /// <summary>
    /// Prints a list of supported local commands to stdout.
    /// </summary>
    public static void PrintCommands()
    {
        Console.WriteLine("Supported commands:");
        Console.WriteLine("----/auth {USERNAME} {SECRET} {DISPLAY_NAME} : Authenticate to the server.");
        Console.WriteLine("----/join {CHANNEL_ID} : Join a channel.");
        Console.WriteLine("----/rename {DISPLAY_NAME} : Rename yourself.");
        Console.WriteLine("----/help : Show this help message.");
        Console.WriteLine("----/status : Show current connection status.");
    }
}