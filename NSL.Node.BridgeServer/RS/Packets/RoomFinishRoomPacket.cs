﻿using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class RoomFinishRoomPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var lobbyServerIdentity = data.ReadString16();

            byte[] dataBuffer = data.Read(data.DataLength - data.DataPosition);

            client.Entry.LobbyManager.SendLobbyFinishRoom(lobbyServerIdentity, dataBuffer);
        }
    }
}
