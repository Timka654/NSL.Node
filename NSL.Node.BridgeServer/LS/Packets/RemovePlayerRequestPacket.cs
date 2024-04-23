using NSL.Node.BridgeServer.Shared.Requests;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;

namespace NSL.Node.BridgeServer.LS.Packets
{
    internal class RemovePlayerRequestPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var request = LobbyRoomPlayerRemoveRequestModel.ReadFullFrom(data);

            client.RemovePlayerId(request.RoomId, request.PlayerId);

            client.Network.Send(packet);
        }
    }
}
