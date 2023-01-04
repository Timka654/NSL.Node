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
        private void TransportPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var body = buffer.GetBuffer();

            var to = buffer.ReadGuid();

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(NodeTransportPacketEnum.Transport);

            pbuf.WriteGuid(client.Id);

            pbuf.Write(body[0..7]);
            pbuf.Write(body[24..]);

            client.Room.SendTo(to, pbuf);
        }
    }
}
