using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Extensions.Buffer;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NSL.Node.BridgeServer.Utils;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.LS.Packets
{
    internal class CreateRoomSessionRequestPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var roomId = data.ReadGuid();

            var roomStartupInfo = data.ReadCollection(x=>new KeyValuePair<>)

            bool result = client.Entry.LobbyManager.TryLobbyServerConnect(client, data.ReadString16());

            packet.WriteBool(result);

            client.Network.Send(packet);
        }
    }
}
