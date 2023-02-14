namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeTransportPacketEnum : ushort
    {
        SignServerPID = 1,
        SignServerResultPID = SignServerPID,
        SignSessionPID,
        SignSessionResultPID = SignSessionPID
    }
}
