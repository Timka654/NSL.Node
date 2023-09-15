using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Utils;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;

namespace NSL.Node.BridgeServer.LS.Packets
{
    internal class AddPlayerRequestPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var request = LobbyRoomPlayerAddRequestModel.ReadFullFrom(data);

            client.AddPlayerId(request.RoomId, request.PlayerId);

            client.Network.Send(packet);
        }
    }
}
