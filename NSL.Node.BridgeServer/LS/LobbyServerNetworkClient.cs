using NSL.Node.BridgeServer.Models;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketServer.Utils;
using NSL.WebSockets.Server.AspNetPoint;
using System;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerNetworkClient : AspNetWSNetworkServerClient
    {
        public string Identity { get; set; }

        public bool Signed { get; set; }

        public PacketWaitBuffer RequestBuffer { get; set; }

        internal ConcurrentDictionary<Guid, RoomDataModel> Rooms { get; } = new ConcurrentDictionary<Guid, RoomDataModel>();
        public BridgeServerStartupEntry Entry { get; internal set; }

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
