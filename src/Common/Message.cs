/* This file implements a common interface for messages that are sent between the client and server.

    Author: Igor Lacko (xlackoi00)
*/

namespace src.Common;

/// <summary>
/// Enum representing the message type
/// </summary>
public enum MessageType
{
    AUTH,
    BYE,
    CONFIRM,
    ERR,
    JOIN,
    MSG,
    PING,
    REPLY,
    NONREPLY, // !REPLY
    EMPTY,
    UNKNOWN
}


/// <summary>
/// Structure representing the message parameters. Contains:
/// <list type="bullet">
///    <item> <description> Username: Mandatory for the AUTH message </description> </item>
///    <item> DisplayName: Mandatory for the AUTH, JOIN, ERR, BYE and MSG messages </item>
///    <item> ChannelID: Mandatory for the JOIN message </item>
///    <item> Secret: Mandatory for the AUTH message </item>
///    <item> MessageContent: Mandatory for the MSG, ERR, REPLY and !REPLY messages </item>
/// </list>
/// </summary>
public struct MessageParams
{
    public string? Username;
    public string? DisplayName;
    public string? ChannelID;
    public string? Secret;
    public string? MessageContent;
}
