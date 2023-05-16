using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Models;
using NSL.Node.BridgeServer.RS;
using NSL.WebSockets.Server.AspNetPoint;
using System;

namespace NSL.Node.BridgeServer.CS
{
    public class ClientServerNetworkClient : AspNetWSNetworkServerClient
    {
        public string LobbyServerIdentity { get; set; }

        public string SessionIdentity { get; set; }

        public Guid RoomId { get; internal set; }

        public bool Signed { get; set; }

        internal RoomDataModel Room { get; set; }

        public TransportSession[] TransportSessions { get; set; }

        internal LobbyServerNetworkClient LobbyServer { get; set; }
        public BridgeServerStartupEntry Entry { get; internal set; }
    }
}
