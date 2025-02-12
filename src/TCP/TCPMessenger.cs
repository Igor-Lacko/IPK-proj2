/* This file contains the implementation of a class which sends generic TCP messages to the provided stream. */

using System.Net.Sockets;
using src.Common;
using src.TCP;

public static class TCPMessenger
{
    public static bool SendByeMessage(TextWriter writer, ClientData data)
    {
        if(data.username == null)
        {
            ErrorLogger.ErrorMessage("Username is null, cannot send BYE message");
            return false;
        }

        writer.WriteLine($"BYE FROM {data.display_name}\r\n");
        writer.Flush();
        return true;
    }

    public static bool SendErrMessage(TextWriter writer, string message, ClientData data)
    {
        if(data.username == null || data.display_name == null)
        {
            ErrorLogger.ErrorMessage("Username or display name is null, cannot send ERR message");
            return false;
        }

        writer.WriteLine($"ERR FROM {data.display_name} IS {message}\r\n");
        writer.Flush();
        return true;
    }
}