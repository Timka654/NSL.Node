using NSL.SocketServer.Utils;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerNetworkClient : IServerNetworkClient
    {
        public Guid Id { get; set; }

        public string ConnectionEndPoint { get; set; }

        public ConcurrentDictionary<Guid, TransportSession> SessionMap { get; } = new ConcurrentDictionary<Guid, TransportSession>();
    }

    public class TransportSession
    {
        public TransportSession(string identityKey, Guid roomId)
        {
            IdentityKey = identityKey;
            RoomId = roomId;
        }

        public string IdentityKey { get; }

        public Guid RoomId { get; }

        public Guid TransportIdentity { get; set; }

        public DateTime? LogInTime { get; set; }

        public DateTime CreateTime { get; } = DateTime.UtcNow;
    }
}
