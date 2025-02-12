/* File that contains the implementation and structure of local Client commands. */
using System.Text.RegularExpressions;

namespace src.Common;



/// <summary>
/// Enum representing the possible commands gotten from the user input.
/// </summary>
public enum CommandType
{
    AUTH,                   // /auth <username> <secret> <display_name>
    JOIN,                   // /join <channel_id>
    RENAME,                 // /rename <display_name> 
    HELP,                   // /help
    MSG,                    // Anything not starting with '/'. TODO: Check length? (Done already?)
    INVALID,                // Invalid command, this might not be needed
    EOF                     // End of file, when user presses Ctrl+D
}

/// <summary>
/// Static class containing all the  command patterns as regexes, and a method to translate a raw command into a type.
/// <note> Similiar to MessageParser, but for commands. </note>
/// </summary>
public static class CommandParser
{
    /* ----------Regular expressions for command parsing---------- */

    // 1. The message parameters
    public static readonly string Username = @"[A-Za-z0-9\-]{1,20}";
    public static readonly string Secret = @"[A-Za-z0-9\-]{1,128}";
    public static readonly string DisplayName = @"[\x21-\x7E]{1,20}";
    public static readonly string ChannelID = @"[A-Za-z0-9\-]{1,20}";

    // 2. Individual command types
    public static readonly string AuthRegex = @$"^(?i)/auth ({Username}) ({Secret}) ({DisplayName})$";
    public static readonly string JoinRegex = @$"^(?i)/join ({ChannelID})$";
    public static readonly string RenameRegex = @$"^(?i)/rename ({DisplayName})$";
    public static readonly string HelpRegex = @$"^(?i)/help$";

    // Messages should not start with a '/' and be max 60000 characters long
    public static readonly string MsgRegex = @"^(?!/)[\x20-\x7E]+$";

    /// <summary>
    /// Method to parse a input string into a command structure, with a type and parameters
    /// </summary>
    /// <param name="input">The unprocessed input string</param>
    /// <returns></returns>
    public static Command ParseCommand(string? input)
    {
        // Initialize the command with the string
        Command command = new()
        {
            command = input,

            // Parse the type
            type = input == null ? CommandType.EOF : input switch
            {
                var cmd when Regex.IsMatch(cmd, AuthRegex) => CommandType.AUTH,
                var cmd when Regex.IsMatch(cmd, JoinRegex) => CommandType.JOIN,
                var cmd when Regex.IsMatch(cmd, RenameRegex) => CommandType.RENAME,
                var cmd when Regex.IsMatch(cmd, HelpRegex) => CommandType.HELP,
                var cmd when Regex.IsMatch(cmd, MsgRegex) => CommandType.MSG,
                _ => CommandType.INVALID
            }
        };

        // Truncate messages that are longer than 60000
        if(command.type == CommandType.MSG)
        {
            if(input!.Length > 60000)
            {
                // TODO: Figure out how to handle this
                Console.WriteLine($"ERROR: {input.Length} characters is too long for a message. Truncating to 60000 characters.");
                command.command = input[..60000];
            }
        }

        // Extract the parameters
        command.parameters = command.type switch
        {
            CommandType.AUTH => new CommandParams
            {
                username = Regex.Match(input!, AuthRegex).Groups[1].Value,
                secret = Regex.Match(input!, AuthRegex).Groups[2].Value,
                display_name = Regex.Match(input!, AuthRegex).Groups[3].Value
            },
            CommandType.JOIN => new CommandParams
            {
                channel_id = Regex.Match(input!, JoinRegex).Groups[1].Value
            },
            CommandType.RENAME => new CommandParams
            {
                display_name = Regex.Match(input!, RenameRegex).Groups[1].Value
            },
            _ => new CommandParams()
        };

        return command;
    }
}

/// <summary>
/// Structure representing the parameters of a command. Most will be empty.
/// </summary>
public struct CommandParams
{
    public string? username;
    public string? secret;
    public string? display_name;
    public string? channel_id;
}

/// <summary>
/// Struct representing a  command, contains a type and the raw input.
/// </summary>
public struct Command
{
    public CommandType type;
    public CommandParams parameters;
    public string? command;

    /// <summary>
    /// Wrapper to allow construction using the CommandParser.ParseCommand method
    /// </summary>
    /// <param name="input">Input string</param>
    public Command(string? input) => CommandParser.ParseCommand(input);
}