using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Node.RoomServer.Bridge
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
