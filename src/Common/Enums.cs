/* Contains some commonly used enumerations. */


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