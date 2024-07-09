using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.SocketCore.Extensions.Buffer;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer.LS.Packets
{
    internal class CreateRoomSessionRequestPacket
    {
        public static async Task ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            var request = LobbyCreateRoomSessionRequestModel.ReadFullFrom(data);

            var result = await client.Entry.RoomManager.CreateRoomSession(client, request);

            result.WriteFullTo(response);

            client.Network.Send(response);
        }
    }
}
