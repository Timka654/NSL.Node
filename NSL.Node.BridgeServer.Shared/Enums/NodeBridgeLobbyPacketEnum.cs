namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeLobbyPacketEnum : ushort
    {
        SignServerRequest = 1,
        ValidateSessionRequest = 2,
        RoomStartupInfoRequest = 3,
        FinishRoomMessage = 4,
        Response = 5
    }
}
