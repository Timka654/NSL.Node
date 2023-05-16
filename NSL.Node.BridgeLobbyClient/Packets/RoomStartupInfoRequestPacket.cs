using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeServer.Shared;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Node.BridgeLobbyClient.Packets
{
    internal class RoomStartupInfoRequestPacket
    {
        public static async void Handle(BridgeLobbyNetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateWaitBufferResponse().WithPid(NodeBridgeLobbyPacketEnum.Response);

            var roomId = data.ReadGuid();
            bool result = default;

            NodeRoomStartupInfo startupInfo = new NodeRoomStartupInfo();

            if (client.HandlesConfiguration.RoomStartupInfoHandle != null)
                result = await client.HandlesConfiguration.RoomStartupInfoHandle(roomId, startupInfo);

            packet.WriteBool(result);

            if (result)
                packet.WriteCollection(startupInfo.GetCollection(), item =>
                {
                    packet.WriteString16(item.Key);
                    packet.WriteString16(item.Value);
                });

            client.Network.Send(packet);
        }
    }
}
