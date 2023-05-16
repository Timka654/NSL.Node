using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class RoomStartupInfoPacket
    {
        public static async void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeRoomPacketEnum.Response);

            var lobbyServerIdentity = data.ReadString16();
            var roomId = data.ReadGuid();

            var result = await client.Entry.LobbyManager.GetRoomStartupInfo(lobbyServerIdentity, roomId);

            packet.WriteBool(result.result);

            if (result.result)
                packet.Write(result.data);

            client.Send(packet);
        }
    }
}
