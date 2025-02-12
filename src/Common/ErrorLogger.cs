/* File containing error logger for the project to print error/debug messages */

namespace src.Common;

public static class ErrorLogger
{
    public static void ErrorMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        // Reset the color
        Console.ResetColor();
    }

    public static void DebugPrint(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"[DEBUG] {message}");
        // Reset the color
        Console.ResetColor();
    }
}