using NSL.WebSockets.Server.AspNetPoint;
using System;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerNetworkClient : AspNetWSNetworkServerClient
    {
        public Guid Id { get; set; }

        public bool Signed { get; set; }

        public string ConnectionEndPoint { get; set; }

        private ConcurrentDictionary<Guid, TransportSession> SessionMap { get; } = new ConcurrentDictionary<Guid, TransportSession>();
        public BridgeServerStartupEntry Entry { get; internal set; }

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

        public Guid TransportIdentity { get; set; }

        public event Action<TransportSession> OnDestroy = session => { };

        public void Dispose()
        {
            OnDestroy(this);
        }
    }
}
