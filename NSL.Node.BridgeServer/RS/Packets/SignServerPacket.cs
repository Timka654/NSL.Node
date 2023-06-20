using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Extensions.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NSL.Node.BridgeServer.Utils;
using NSL.Node.BridgeServer.Shared.Requests;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class SignServerPacket
    {
        public static string GetIdentityKey(NetworkClient client) => client.Entry.Configuration.GetValue<string>("transport_server_identityKey", "AABBCC");

        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateResponse();

            var request = RoomSignInRequestModel.ReadFullFrom(data);


            client.Id = request.Identity;

            client.ConnectionEndPoint = request.ConnectionEndPoint;

            if (!GetIdentityKey(client).Equals(request.IdentityKey))
            {
                response.WriteBool(false);

                client.Send(response);

                return;
            }

            bool result = client.Entry.RoomManager.TryRoomServerConnect(client);

            response.WriteBool(result);

            if (result)
                response.WriteGuid(client.Id);

            client.Send(response);
        }
    }
}
