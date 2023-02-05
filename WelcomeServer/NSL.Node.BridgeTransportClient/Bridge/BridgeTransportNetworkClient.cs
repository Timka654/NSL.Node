using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Bridge
{
    public class BridgeTransportNetworkClient : BaseSocketNetworkClient
    {
        public PacketWaitBuffer PacketWaitBuffer { get; }

        public BridgeTransportNetworkClient()
        {
            PacketWaitBuffer = new PacketWaitBuffer(this);
        }

        public override void Dispose()
        {
            PacketWaitBuffer.Dispose();

            base.Dispose();
        }
    }
}
