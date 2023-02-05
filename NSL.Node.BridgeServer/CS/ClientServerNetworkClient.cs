using NSL.SocketServer.Utils;

namespace NSL.Node.BridgeServer.CS
{
    public class ClientServerNetworkClient : IServerNetworkClient
    {
        public string ServerIdentity { get; set; }

        public string SessionIdentity { get; set; }

        public Guid RoomId { get; internal set; }
    }
}
