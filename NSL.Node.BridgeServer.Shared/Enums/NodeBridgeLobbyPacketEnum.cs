namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeLobbyPacketEnum : ushort
    {
        Response = 1,
        SignServerRequest,
        ValidateSessionRequest,
        CreateRoomSessionRequest,
        FinishRoomMessage,
        RoomMessage
    }
}
