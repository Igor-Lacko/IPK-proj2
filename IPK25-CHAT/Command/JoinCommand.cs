/* Contains the JOIN command class */

namespace IPK_25_CHAT.Command;

using IPK_25_CHAT.Enum;

/// <summary>
/// JOIN command class.
/// </summary>
/// <param name="channelId">Channel ID to join.</param>
public class JoinCommand(string channelId) : Command(CommandType.JOIN)
{
    /// <summary>
    /// Regular expression for the JOIN command.
    /// </summary>
    public static string Format = @$"^/join\s+{ParameterFormats.CHANNEL_ID}$";

    /// <summary>
    /// Channel ID to join.
    /// </summary>
    public readonly string ChannelId = channelId;

    /// <summary>
    /// Validates the join command. Is only valid when connected to a channel, e.g. in the OPEN state.
    /// <summary>
    /// <param name="clientState">The current state of the client</param>
    public override bool IsValid(State clientState) => clientState == State.OPEN;

    /// <summary>
    /// Toggles the format to use the discord notation.
    /// </summary>
    public static void ToggleDiscordNotation()
    {
        Format = $@"^/join\s+{ParameterFormats.DISCORD_CHANNEL_ID}$";
    }
}