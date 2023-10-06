using System;

/// <summary>
/// Represents the neccesary information for a player to host a game on a relay.
/// </summary>
public struct RelayHostData
{
    public string joinCode;
    public string ipV4Address;
    public ushort port;
    public Guid allocationId;
    public byte[] allocationIdBytes;
    public byte[] connectionData;
    public byte[] key;
}

/// <summary>
/// Represents the neccesary information for a player to join a game on a relay.
/// </summary>
public struct RelayJoinData
{
    public string joinCode;
    public string ipV4Address;
    public ushort port;
    public Guid allocationId;
    public byte[] allocationIdBytes;
    public byte[] connectionData;
    public byte[] hostConnectionData;
    public byte[] key;
}
