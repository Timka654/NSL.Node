using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;
using System;
using System.Threading;
using NSL.UDP.Enums;
using NSL.UDP;

namespace NSL.Node.P2Proxy.Client.Data
{
    public class P2PNetworkClient : IServerNetworkClient
    {
        public Guid Id { get; set; }

        public ProxyRoomInfo Room { get; set; }
    }
}
