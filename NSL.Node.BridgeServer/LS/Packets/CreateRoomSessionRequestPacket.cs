using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NSL.Node.BridgeServer.Utils;
using NSL.Node.BridgeServer.Shared.Requests;

namespace NSL.Node.BridgeServer.LS.Packets
{
    internal class CreateRoomSessionRequestPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            var request = LobbyCreateRoomSessionRequestModel.ReadFullFrom(data);

            var result = client.Entry.RoomManager.CreateRoomSession(client, request);

            result.WriteFullTo(response);

            client.Network.Send(response);
        }
    }
}
