using NSL.Node.BridgeServer.Models;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketServer.Utils;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerNetworkClient : IServerNetworkClient
    {
        public string Identity { get; set; }

        public bool Signed { get; set; }

        public PacketWaitBuffer RequestBuffer { get; set; }

        internal ConcurrentDictionary<Guid, RoomDataModel> Rooms { get; } = new ConcurrentDictionary<Guid, RoomDataModel>();

        public LobbyServerNetworkClient() : base()
        {
            RequestBuffer = new PacketWaitBuffer(this);
        }

        public override void Dispose()
        {
            if (RequestBuffer != null)
                RequestBuffer.Dispose();

            base.Dispose();
        }

    }
}
