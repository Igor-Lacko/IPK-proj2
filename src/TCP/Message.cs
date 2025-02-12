/* This file contains the implementation of message strucutres/classes for the TCP variant of the IPK25 chat protocol
    Author: Igor Lacko (xlackoi00)
*/

using src.Common;

namespace src.TCP;


/// <summary>
/// Struct representing a TCP message, contains a type and the raw input.
/// </summary>
/// <param name="message">String representation of the message</param>
public readonly struct TCPMessage(string? message, MessageParams parameters = new())
{
    public MessageType Type { get; } = TCPMessageParser.ParseMessageType(message);
    public MessageParams Params { get; } = parameters;
    public string RawInputMessage { get; } = message == null ? "\r\n" : message + "\r\n";

}
