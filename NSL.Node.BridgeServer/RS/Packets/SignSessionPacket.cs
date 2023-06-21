using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.Node.BridgeServer.Utils;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class SignSessionPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var request = RoomSignSessionRequestModel.ReadFullFrom(data);

            RoomSignSessionResponseModel result = new RoomSignSessionResponseModel();

            var session = client.GetSession(request.SessionIdentity);

            result.Result = session != null && session.RoomIdentity.Equals(request.RoomIdentity);

            if (result.Result)
            {
                session.Active = true;

                result.Options = session.StartupInfo.GetDictionary();
            }

            result.WriteFullTo(packet);

            client.Send(packet);
        }
    }
}
