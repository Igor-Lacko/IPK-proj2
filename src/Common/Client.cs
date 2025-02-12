/* Contains some commonly used enumerations and data shared between the client variants. */


namespace src.Common;


public enum NetworkProtocol
{
    TCP,
    UDP,
    NONE,
    INVALID
}

public enum ClientState
{
    START,
    AUTH,
    OPEN,
    JOIN,
    END
}

/// <summary>
/// Contains some current non-static data (which can be changed during runtime) used by the Client.
/// </summary>
public struct ClientData
{
    public string? username;
    public string? display_name;
    public string? channel_id;
}