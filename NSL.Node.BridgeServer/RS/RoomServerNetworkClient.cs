using NSL.Node.BridgeServer.CS;
using NSL.SocketServer.Utils;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerNetworkClient : IServerNetworkClient
    {
        public Guid Id { get; set; }

        public bool Signed { get; set; }

        public string ConnectionEndPoint { get; set; }

        private ConcurrentDictionary<Guid, TransportSession> SessionMap { get; } = new ConcurrentDictionary<Guid, TransportSession>();

        public bool TryAddSession(TransportSession session)
        {
            if (SessionMap.TryAdd(session.TransportIdentity, session))
            {
                session.OnDestroy += _ => TryRemoveSession(session.TransportIdentity);
                return true;
            }

            return false;
        }

        public void TryRemoveSession(Guid id)
        {
            if (SessionMap.TryRemove(id, out var session))
                session.Dispose();
        }

        public TransportSession GetSession(Guid sessionId)
        {
            SessionMap.TryGetValue(sessionId, out var session);

            return session;
        }
    }

    public class TransportSession : IDisposable
    {
        public TransportSession(ClientServerNetworkClient client)
        {
            Client = client;
        }

        public Guid TransportIdentity { get; set; }

        public ClientServerNetworkClient Client { get; }

        public event Action<TransportSession> OnDestroy = session => { };

        public void Dispose()
        {
            OnDestroy(this);
        }
    }
}
