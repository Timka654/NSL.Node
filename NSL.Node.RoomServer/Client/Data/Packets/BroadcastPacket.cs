using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils.Buffer;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        private void BroadcastPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var body = buffer.GetBuffer();

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(RoomPacketEnum.Transport);

            pbuf.WriteGuid(client.Id);

            pbuf.Write(body);

            client.Room.Broadcast(pbuf);
        }
    }
}
