using NSL.SocketServer.Utils;
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

        private ConcurrentDictionary<Guid, RoomSession> SessionMap { get; set; } = new ConcurrentDictionary<Guid, RoomSession>();

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

        public override void ChangeOwner(IServerNetworkClient from)
        {
            if (from is not RoomServerNetworkClient other)
                throw new InvalidOperationException($"{nameof(ChangeOwner)} invalid {nameof(from)} type");

            SessionMap = other.SessionMap;

            foreach (var item in SessionMap)
            {
                item.Value.OwnedRoomNetwork = this;
            }

            base.ChangeOwner(from);
        }

        public void Disconnect()
        {
            foreach (var item in SessionMap.Values)
            {
                if (item.OwnedRoomNetwork != this)
                    continue;

                item.SendLobbyFinishRoom(null, false);
                item.Dispose();
            }
        }
    }
}
