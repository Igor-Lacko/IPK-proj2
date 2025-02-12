
namespace src.TCP;
using System.Text.RegularExpressions;
using src.Common;
using src.TCP;

public static class TCPMessageParser
{
     /* ----------Regular expressions for message parsing---------- */

    // 1. Generic definitions (id, message content, display name...)
    public static readonly string ID = @"[A-Za-z0-9\-]{1,20}";
    public static readonly string SECRET = @"[A-Za-z0-9\-]{1,128}";
    public static readonly string CONTENT =  @"[\x20-\x7E]{1,60000}";
    public static readonly string DNAME = @"[\x21-\x7E]{1,20}";

    // 2. Additional components
    public static readonly string IS = @" IS ";
    public static readonly string AS = @" AS ";
    public static readonly string USING = @" USING ";

    // 3. Individual message types
    public static readonly string AuthRegex = @$"^(?i)AUTH {ID}{AS}{DNAME}{USING}{SECRET}$";
    public static readonly string JoinRegex = @$"^(?i)JOIN {ID}{AS}{DNAME}$";
    public static readonly string MsgRegex = @$"^(?i)MSG FROM {DNAME}{IS}{CONTENT}$";
    public static readonly string ErrRegex = @$"^(?i)ERR FROM {DNAME}{IS}{CONTENT}$";
    public static readonly string ByeRegex = @$"^(?i)BYE FROM {DNAME}$";
    public static readonly string ReplyRegex = @$"^(?i)REPLY OK{IS}{CONTENT}$";
    public static readonly string NonReplyRegex = @$"^(?i)REPLY NOK{IS}{CONTENT}$";


    // Method to translate a raw message into a type
    public static MessageType ParseMessageType(string? message)
    {
        if(message == null) return MessageType.UNKNOWN;
        return message switch
        {
            var msg when Regex.IsMatch(msg, AuthRegex) => MessageType.AUTH,
            var msg when Regex.IsMatch(msg, JoinRegex) => MessageType.JOIN,
            var msg when Regex.IsMatch(msg, MsgRegex) => MessageType.MSG,
            var msg when Regex.IsMatch(msg, ErrRegex) => MessageType.ERR,
            var msg when Regex.IsMatch(msg, ByeRegex) => MessageType.BYE,
            var msg when Regex.IsMatch(msg, ReplyRegex) => MessageType.REPLY,
            var msg when Regex.IsMatch(msg, NonReplyRegex) => MessageType.NONREPLY,
            _ => MessageType.UNKNOWN
        };
    }

    // Method to translate a TCP command into a message (when appropriate).
    public static TCPMessage CommandToMessage(Command command, ClientData data)
    {
        MessageParams parameters;
        switch(command.type)
        {
            // Explicitely define the message params and the string
            case CommandType.AUTH:
                parameters = new()
                {
                    username = command.parameters.username,
                    display_name = command.parameters.display_name,
                    secret = command.parameters.secret
                };
                return new TCPMessage($"AUTH {parameters.username} AS {parameters.display_name} USING {parameters.secret}", parameters);

            case CommandType.JOIN:
                parameters = new()
                {
                    channel_id = command.parameters.channel_id,
                    display_name = data.display_name
                };
                return new TCPMessage($"JOIN {parameters.channel_id} AS {parameters.display_name}", parameters);

            case CommandType.MSG:
                parameters = new()
                {
                    display_name = data.display_name,
                    message_content = command.command
                };
                return new TCPMessage($"MSG FROM {parameters.display_name} IS {parameters.message_content}\r\n", parameters);

            // This will never happen, but the compiler requires it
            default:
                throw new InvalidCastException("Only the AUTH, JOIN and MSG commands can be translated to messages. Fix your code!");

        }
    }
}