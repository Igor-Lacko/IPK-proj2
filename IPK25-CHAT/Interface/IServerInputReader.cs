/* Contains the common interface for classes that asynchronously wait for server input */

namespace IPK_25_CHAT.Interface;

using IPK_25_CHAT.Client;

/// <summary>
/// Interface for classes that asynchronously wait for server input.
/// </summary>
public interface IServerInputReader
{
    /// <summary>
    /// Reads input from the server.
    /// </summary>
    /// <returns>A Message object representing server input.</returns>
    public Task<Message> ReadInput();
}