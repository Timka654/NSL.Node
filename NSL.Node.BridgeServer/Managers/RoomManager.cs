using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.RS;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Node.BridgeServer.Managers
{
    public class RoomManager
    {
        public RoomManager(string identityKey)
        {
            this.identityKey = identityKey;
        }

        public void OnDisconnectedRoomServer(RoomServerNetworkClient client)
        {
            if (client.Signed)
                connectedServers.Remove(client.Id, out _);
        }

        public bool TryRoomServerConnect(RoomServerNetworkClient client, string identityKey)
        {
            if (!object.Equals(this.identityKey, identityKey))
                return false;

            if (!Guid.Empty.Equals(client.Id))
            {
                if (connectedServers.TryGetValue(client.Id, out var server))
                {
                    if (server.GetState())
                    {
                        return false;
                    }

                    connectedServers[client.Id] = client;

                    server.ChangeOwner(client);

                    client.Signed = true;

                    return true;
                }
            }
            else
                client.Id = Guid.NewGuid();

            while (!connectedServers.TryAdd(client.Id, client))
            {
                client.Id = Guid.NewGuid();
            }

            client.Signed = true;

            return true;
        }

        public CreateRoomSessionResponseModel CreateRoomSession(LobbyServerNetworkClient client, LobbyCreateRoomSessionRequestModel request)
        {
            var result = new CreateRoomSessionResponseModel();

            RoomServerNetworkClient[] servers = default;

            if (request.SpecialServer.HasValue)
            {
                result.Result = connectedServers.TryGetValue(request.SpecialServer.Value, out var server);

                if (result.Result)
                    servers = Enumerable.Repeat(server, 1).ToArray();
            }
            else
            {
                IEnumerable<RoomServerNetworkClient> serverSelector = connectedServers.Values;

                if (request.Location != default)
                    serverSelector = serverSelector.Where(request.Location.Equals);

                servers = serverSelector.OrderBy(x => x.SessionsCount).Take(request.NeedPointCount).ToArray();


                result.Result = servers.Any();
            }

            if (result.Result)
            {
                var sessions = new List<RoomSession>();

                result.ConnectionPoints = servers.Select(server =>
                {
                    var session = server.CreateSession(new RoomSession(request.RoomId, client, server)
                    {
                        StartupInfo = new Shared.NodeRoomStartupInfo(request.StartupOptions),
                        PlayerIds = request.InitialPlayers
                    });

                    session.OnDestroy += session =>
                    {
                        sessions.Remove(session);

                        if (sessions.Any() == false)
                            client.Rooms.Remove(session.RoomIdentity, out _);
                    };

                    sessions.Add(session);

                    return new Shared.RoomServerPointInfo()
                    {
                        Endpoint = server.ConnectionEndPoint,
                        SessionId = session.SessionId
                    };
                }).ToList();

                client.Rooms.AddOrUpdate(request.RoomId, k => sessions, (k, o) => sessions);
            }

            return result;
        }

        private ConcurrentDictionary<Guid, RoomServerNetworkClient> connectedServers = new ConcurrentDictionary<Guid, RoomServerNetworkClient>();
        private readonly string identityKey;
    }

    public record CreateSignResult(string endPoint, Guid id);
}
