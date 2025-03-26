/* Contains a class for printing warning/error messages to the user. */

namespace IPK_25_CHAT.Error;

/// <summary>
/// Class for printing warning/error messages to the user.
/// </summary>
public static class ErrorLogger
{
    /// <summary>
    /// Prints a warning message to the console (in yellow!).
    /// </summary>
    /// <param name="message">Warning message.</param>
    public static void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"WARNING: {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints an error message to the console (in red!) and exits the program with the given code.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="exitCode">Code to exit the program with.</param>
    public static void Error(string message, int exitCode)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: {message}");
        Console.ResetColor();
        Environment.Exit(exitCode);
    }
}