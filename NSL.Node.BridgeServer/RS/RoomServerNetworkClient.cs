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

        public string Location { get; set; }

        public int SessionsCount => SessionMap.Count;

        private ConcurrentDictionary<Guid, RoomSession> SessionMap { get; } = new ConcurrentDictionary<Guid, RoomSession>();

        public NodeBridgeServerEntry Entry { get; internal set; }

        public RoomSession CreateSession(RoomSession session)
        {
            while (!SessionMap.TryAdd(session.SessionId = Guid.NewGuid(), session)) ;

            session.OnDestroy += session => TryRemoveSession(session.SessionId);

            return session;
        }

        public void TryRemoveSession(Guid sessionId)
        {
            if (SessionMap.TryRemove(sessionId, out var session))
                session.Dispose();
        }

        public RoomSession GetSession(Guid sessionId)
        {
            SessionMap.TryGetValue(sessionId, out var session);

            return session;
        }
    }
}
