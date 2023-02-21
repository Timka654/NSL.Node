using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class SignSessionPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeRoomPacketEnum.SignSessionResultPID);

            var identityKey = data.ReadString16();
            var id = data.ReadGuid();

            var session = client.GetSession(id);

            if (session == null || !session.Client.SessionIdentity.Equals(identityKey))
            {
                packet.WriteBool(false);

                client.Send(packet);

                return;
            }

            packet.WriteBool(true);

            packet.WriteString16(session.Client.LobbyServerIdentity);
            packet.WriteGuid(session.Client.RoomId);

            client.Send(packet);
        }
    }
}
