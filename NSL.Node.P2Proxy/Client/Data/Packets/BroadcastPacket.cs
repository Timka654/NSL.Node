using NSL.Node.P2Proxy.Client.Data;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.P2Proxy.Client
{
    public partial class P2ProxyEntry
    {
        private void BroadcastPacketHandle(P2PNetworkClient client, InputPacketBuffer buffer)
        {
            client.Room?.Broadcast(client, buffer);
            //OutputPacketBuffer pbuf = OutputPacketBuffer.Create(RoomPacketEnum.Transport);

            //pbuf.WriteGuid(client.Id);

            //pbuf.Write(body);

            //client.Room.Broadcast(pbuf);
        }
    }
}
