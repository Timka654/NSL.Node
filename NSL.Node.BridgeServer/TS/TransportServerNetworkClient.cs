using NSL.SocketServer.Utils;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.TS
{
    internal class TransportServerNetworkClient : IServerNetworkClient
    {
        public Guid Id { get; set; }

        public string PublicIPAddr { get; set; }

        public int PublicPort { get; set; }

        public ConcurrentDictionary<Guid, TransportSession> SessionMap { get; } = new ConcurrentDictionary<Guid, TransportSession>();
    }

    internal class TransportSession
    {
        public string IdentityKey { get; set; }

        public DateTime? LogInTime { get; set; }

        public DateTime CreateTime { get; } = DateTime.UtcNow;
    }
}
