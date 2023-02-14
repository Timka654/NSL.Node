using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Node.BridgeLobbyClient
{
    public class BridgeLobbyNetworkClient : BaseSocketNetworkClient
    {
        public PacketWaitBuffer PacketWaitBuffer { get; }

        public BridgeLobbyNetworkClient()
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
