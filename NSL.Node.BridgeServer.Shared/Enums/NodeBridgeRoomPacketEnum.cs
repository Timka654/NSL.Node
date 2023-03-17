namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeRoomPacketEnum : ushort
    {
        SignServerRequest = 1,
        SignSessionRequest = 2,
        RoomStartupInfoRequest = 3,
        FinishRoomMessage = 4,
        Response
    }
}
