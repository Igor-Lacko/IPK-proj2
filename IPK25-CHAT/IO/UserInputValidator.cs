/* Class that validates user input for the client, also checks for validity via states */

namespace IPK_25_CHAT.IO;

using IPK_25_CHAT.Client;
using IPK_25_CHAT.Interface;
using IPK_25_CHAT.Enum;
using IPK_25_CHAT.Message;
using IPK_25_CHAT.Command;

/// <summary>
/// Validates the current user input for the client. Keeps track of it's state via events.
/// </summary>
public class UserInputValidator
{
    /// <summary>
    /// Current state of the client. Updated via events.
    /// </summary>
    public State ClientState { get; set; } = State.START;

    /// <summary>
    /// User session settings (username, etc.), for constructing Msg messages.
    /// Gotten from the client on construction.
    /// </summary>
    public UserSessionConfiguration Config { get; set; } = new();

    /// <summary>
    /// Validates the current user input and returns it as a IReadable object.
    /// </summary>
    /// <param name="input">Given user input.</param>
    /// <returns>A IReadable object representing the given input.</returns>
    public IReadable? Validate(string input)
    {
        Console.WriteLine($"Validating input: {input}");
        // Truncate the message to 60000 characters
        if(input.Length > 60000)
            input = input[..60000];

        // Try to parse as a command first
        if(Command.Parse(input, out Command? command))
        {
            // Validate the command
            if(command!.IsValid(ClientState))
                return command;

            // Print a error message to the user
            StdoutResultWriter.InternalClientError($"ERROR: {command}");
        }

        // Try to look for an invalid command
        else if(input.StartsWith('/'))
        {
            StdoutResultWriter.InternalClientError($"ERROR: {input}");
            return null;
        }

        // Consider the input as a message
        if(Config.DisplayName == null)
        {
            StdoutResultWriter.InternalClientError($"ERROR: {input}");
            return null;
        }

        // Check if the message is valid
        MsgMessage msg = new(Config.DisplayName, input);
        if(!msg.IsValid(ClientState))
        {
            StdoutResultWriter.InternalClientError($"ERROR: {input}");
            return null;
        }

        return msg;
    }
}