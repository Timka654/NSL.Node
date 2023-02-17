using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Models;
using NSL.Node.BridgeServer.RS;
using NSL.SocketServer.Utils;

namespace NSL.Node.BridgeServer.CS
{
    public class ClientServerNetworkClient : IServerNetworkClient
    {
        public string LobbyServerIdentity { get; set; }

        public string SessionIdentity { get; set; }

        public Guid RoomId { get; internal set; }

        public bool Signed { get; set; }

        internal RoomDataModel Room { get; set; }

        public TransportSession[] TransportSessions { get; set; }

        internal LobbyServerNetworkClient LobbyServer { get; set; }
    }
}
