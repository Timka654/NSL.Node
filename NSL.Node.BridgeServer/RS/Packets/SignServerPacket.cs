using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NSL.Node.BridgeServer.Utils;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class SignServerPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            var request = RoomSignInRequestModel.ReadFullFrom(data);


            client.Id = request.Identity;

            client.ConnectionEndPoint = request.ConnectionEndPoint;

            bool result = client.Entry.RoomManager.TryRoomServerConnect(client, request.IdentityKey);

            new RoomSignInResponseModel { Result = result, ServerIdentity = client.Id}.WriteFullTo(response);

            client.Send(response);
        }
    }
}
