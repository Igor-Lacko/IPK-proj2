/* File that contains the implementation and structure of TCP commands. */
using System.Text.RegularExpressions;

namespace src.TCP;




/// <summary>
/// Enum representing the possible commands gotten from the user input.
/// </summary>
public enum TCPCommandType
{
    AUTH,                   // /auth <username> <secret> <display_name>
    JOIN,                   // /join <channel_id>
    RENAME,                 // /rename <display_name> 
    HELP,                   // /help
    MSG,                    // Anything not starting with '/'. TODO: Check length
    INVALID                 // Invalid command, this might not be needed
}

/// <summary>
/// Static class containing all the TCP command patterns as regexes, and a method to translate a raw command into a type.
/// <note> Similiar to TCPMessageParser, but for commands. </note>
/// </summary>
public static class TCPCommandParser
{
    /* ----------Regular expressions for command parsing---------- */

    // 1. The message parameters
    public static readonly string Username = @"[A-Za-z0-9\-]{1,20}";
    public static readonly string Secret = @"[A-Za-z0-9\-]{1,128}";
    public static readonly string DisplayName = @"[\x21-\x7E]{1,20}";
    public static readonly string ChannelID = @"[A-Za-z0-9\-]{1,20}";

    // 2. Individual command types
    public static readonly string AuthRegex = @$"^(?i)/auth {Username} {Secret} {DisplayName}$";
    public static readonly string JoinRegex = @$"^(?i)/join {ChannelID}$";
    public static readonly string RenameRegex = @$"^(?i)/rename {DisplayName}$";
    public static readonly string HelpRegex = @$"^(?i)/help$";

    // Messages should not start with a '/' and be max 60000 characters long
    public static readonly string MsgRegex = @"^(?!/)[\x20-\x7E]{1,60000}$";

    // Method to translate a raw command into a type
    public static TCPCommandType ParseCommandType(string? command)
    {
        if(command == null) return TCPCommandType.INVALID;
        return command switch
        {
            var cmd when Regex.IsMatch(cmd, AuthRegex) => TCPCommandType.AUTH,
            var cmd when Regex.IsMatch(cmd, JoinRegex) => TCPCommandType.JOIN,
            var cmd when Regex.IsMatch(cmd, RenameRegex) => TCPCommandType.RENAME,
            var cmd when Regex.IsMatch(cmd, HelpRegex) => TCPCommandType.HELP,
            var cmd when Regex.IsMatch(cmd, MsgRegex) => TCPCommandType.MSG,
            _ => TCPCommandType.INVALID
        };
    }
}

/// <summary>
/// Struct representing a TCP command, contains a type and the raw input.
/// </summary>
/// <param name="command">The string representation of trhe command, translated to a type.</param>
public readonly struct TCPCommand(string? command)
{
    public TCPCommandType Type { get; } = TCPCommandParser.ParseCommandType(command);
    public string? RawCommand { get; } = command;
}