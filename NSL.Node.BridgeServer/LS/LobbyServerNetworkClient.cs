using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketServer.Utils;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerNetworkClient : IServerNetworkClient
    {
        public string Identity { get; set; }

        public PacketWaitBuffer ValidateRequestBuffer { get; set; }

        public LobbyServerNetworkClient() : base()
        {
            ValidateRequestBuffer = new PacketWaitBuffer(this);
        }

        public override void Dispose()
        {
            if (ValidateRequestBuffer != null)
                ValidateRequestBuffer.Dispose();

            base.Dispose();
        }

    }
}
