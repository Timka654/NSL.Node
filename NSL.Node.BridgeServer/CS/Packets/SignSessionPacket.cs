using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;

using NetworkClient = NSL.Node.BridgeServer.CS.ClientServerNetworkClient;

namespace NSL.Node.BridgeServer.CS.Packets
{
    internal class SignSessionPacket
    {
        internal static async void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            client.LobbyServerIdentity = data.ReadString16();
            client.RoomId = data.ReadGuid();
            client.SessionIdentity = data.ReadString16();

            client.Signed = await client.Entry.LobbyManager.ValidateClientSession(client);

            var packet = OutputPacketBuffer.Create(NodeBridgeClientPacketEnum.SignSessionResultPID);

            packet.WriteBool(client.Signed);

            if (client.Signed)
            {
                client.Entry.RoomManager.CreateRoom(client);

                var sessions = client.Entry.RoomManager.CreateSignSession(client);

                packet.WriteCollection(sessions, i =>
                {
                    packet.WriteString16(i.endPoint);
                    packet.WriteGuid(i.id);
                });
            }

            client.Network.Send(packet);
        }
    }
}
