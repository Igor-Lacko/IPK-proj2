/* Contains the class of the extra /status command, which shows the current user status */

namespace IPK_25_CHAT.Command;

using System.Net;
using IPK_25_CHAT.Client;
using IPK_25_CHAT.Enum;

/// <summary>
/// Class for the /status command.
/// </summary>
public class StatusCommand() : Command(CommandType.STATUS)
{
    /// <summary>
    /// Regular expression for the /status command.
    /// </summary>
    public const string Format = @"^/status$";

    /// <summary>
    /// Validates the status command. Is always valid.
    /// </summary>
    /// <param name="clientState">Current state of the client.</param>
    /// <returns>True always.</returns>
    public override bool IsValid(State clientState) => true;

    /// <summary>
    /// Prints the current status of the user.
    /// </summary>
    public static void PrintStatus(UserSessionConfiguration config, ushort port, IPAddress host, State clientState)
    {
        Console.WriteLine($"Connected to {host} on port {port}");
        switch (clientState)
        {
            case State.START:
                Console.WriteLine($"Waiting for user input. Please authenticate with /auth <username> <secret> <displayName>");
                return;

            // This output shouldn't be possible
            case State.AUTH:
                Console.WriteLine($"Waiting for authentication.");
                Console.WriteLine("User settings:");
                Console.WriteLine($"\t--Username: {config.Username}");
                Console.WriteLine($"\t--Display name: {config.DisplayName}");
                Console.WriteLine($"\t--No channel joined yet");
                return;

            case State.OPEN:
                Console.WriteLine($"Joined in a chat channel.");
                Console.WriteLine("User settings:");
                Console.WriteLine($"\t--Username: {config.Username}");
                Console.WriteLine($"\t--Display name: {config.DisplayName}");
                Console.WriteLine($"\t--Channel: {config.ChannelID}");
                return;

            // This output also shouldn't be possible
            case State.JOIN:
                Console.WriteLine($"Waiting for the server to join the user.");
                Console.WriteLine("User settings:");
                Console.WriteLine($"\t--Username: {config.Username}");
                Console.WriteLine($"\t--Display name: {config.DisplayName}");
                Console.WriteLine($"Current channel: {config.ChannelID}");
                return;

            // And this one probably also not
            case State.END:
                Console.WriteLine($"Waiting for the client to end...");
                Console.WriteLine("Logging out as:");
                Console.WriteLine($"\t--Username: {config.Username}");
                Console.WriteLine($"\t--Display name: {config.DisplayName}");
                Console.WriteLine($"\t--Last channel: {config.ChannelID}");
                return;
        }
    }
}