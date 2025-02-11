/* This file contains the implementation of message strucutres/classes for the TCP variant of the IPK25 chat protocol
    Author: Igor Lacko (xlackoi00)
*/



using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using src.Common;
namespace src.TCP;

/// <summary>
/// Static class containing all the TCP message patterns.
/// </summary>
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
        if(Regex.IsMatch(message!, ByeRegex))
        {
            Console.WriteLine($"Bye message detected: {message}");
        }
        else
        {
            Console.WriteLine($"Message detected: {message}");
        }
        if(message == null || message == "") return MessageType.EMPTY;
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

    // Method to translate a TCP command into a message (when appropriate)
    public static TCPMessage CommandToMessage(TCPCommand command)
    {
        throw new NotImplementedException("Method not implemented yet");
    }
}

/// <summary>
/// Struct representing a TCP message, contains a type and the raw input. TODO: Might need to add params for individual messages.
/// </summary>
/// <param name="message"></param>
public readonly struct TCPMessage(string? message)
{
    public MessageType Type { get; } = TCPMessageParser.ParseMessageType(message);
    public string RawInputMessage { get; } = message == null ? "\r\n" : message + "\r\n";
}

public static class TCPMessageHandler
{
    public static void SendMessage(TCPMessage message, NetworkStream stream)
    {
        // Write the message to the stream
        Console.WriteLine($"Sending message: {message.RawInputMessage}");
    }
}