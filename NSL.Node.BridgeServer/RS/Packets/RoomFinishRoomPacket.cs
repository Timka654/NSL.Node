using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class RoomFinishRoomPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var lobbyServerIdentity = data.ReadString16();

            byte[] dataBuffer = data.Read(data.DataLength - data.DataPosition);

            var lobby = client.Entry.LobbyManager.GetLobbyById(lobbyServerIdentity);

            if (lobby == null)
                return; // todo

            var packet = OutputPacketBuffer.Create(NodeBridgeLobbyPacketEnum.FinishRoomMessage);

            packet.Write(dataBuffer);

            lobby.Network.Send(packet);

        }
    }
}
