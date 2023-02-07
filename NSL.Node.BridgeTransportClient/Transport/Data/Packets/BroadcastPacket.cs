using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeTransportClient.Transport.Data;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Transport
{
    public partial class TransportNetwork
    {
        private void BroadcastPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var body = buffer.GetBuffer();

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(NodeTransportPacketEnum.Transport);

            pbuf.WriteGuid(client.Id);

            pbuf.Write(body);

            client.Room.Broadcast(pbuf);
        }
    }
}
