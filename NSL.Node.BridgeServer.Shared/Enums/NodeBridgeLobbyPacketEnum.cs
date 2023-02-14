namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeLobbyPacketEnum : ushort
    {
        SignServerPID = 1,
        SignServerResultPID = SignServerPID,
        ValidateSessionPID = 2,
        ValidateSessionResultPID = ValidateSessionPID
    }
}
