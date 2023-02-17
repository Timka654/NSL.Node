﻿namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeTransportPacketEnum : ushort
    {
        SignServerPID = 1,
        SignServerResultPID = SignServerPID,
        SignSessionPID = 2,
        SignSessionResultPID = SignSessionPID,
        RoomStartupInfoPID = 3,
        RoomStartupInfoResultPID = RoomStartupInfoPID,
        FinishRoom = 4
    }
}
