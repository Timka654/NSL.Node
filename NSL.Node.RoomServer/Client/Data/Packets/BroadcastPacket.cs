using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils.Buffer;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerBaseEntry
    {
        private void BroadcastPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var body = buffer.GetBuffer();

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(RoomPacketEnum.TransportMessage);

            pbuf.WriteGuid(client.Id);

            pbuf.Write(body);

            client.Room.Broadcast(pbuf);
        }
    }
}
