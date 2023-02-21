using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Extensions.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class SignServerPacket
    {
        public static string GetIdentityKey(NetworkClient client) => client.Entry.Configuration.GetValue<string>("transport_server_identityKey", "AABBCC");

        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeRoomPacketEnum.SignServerResultPID);

            client.Id = data.ReadGuid();

            client.ConnectionEndPoint = data.ReadString16();

            var serverIdentityKey = data.ReadString16();


            if (!GetIdentityKey(client).Equals(serverIdentityKey))
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
