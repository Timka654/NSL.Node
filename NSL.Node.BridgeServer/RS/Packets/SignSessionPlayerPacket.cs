using NSL.Node.BridgeServer.Shared.Message;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.Node.BridgeServer.Utils;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class SignSessionPlayerPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            var request
                = RoomSignSessionPlayerRequestModel.ReadFullFrom(data);

            var result = new RoomSignSessionPlayerResponseModel();

            var session = client.GetSession(request.SessionId);

            result.ExistsSession = session != null;

            if (session != null)
            {
                session.ValidatePlayer(request.PlayerId);
            }

            result.WriteFullTo(response);

            client.Network?.Send(response);
        }
    }
}
