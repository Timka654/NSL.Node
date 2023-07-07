using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.RS;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer.Managers
{
    public class RoomManager
    {
        public RoomManager(string identityKey)
        {
            this.identityKey = identityKey;
        }

        public async void OnDisconnectedRoomServer(RoomServerNetworkClient client)
        {
            if (!client.Signed)
                return;

            await Task.Delay(2000);

            if (connectedServers[client.Id] != client)
                return;

            connectedServers.Remove(client.Id, out _);

            client.Disconnect();
        }

        public bool TryRoomServerConnect(RoomServerNetworkClient client, RoomSignInRequestModel request)
        {
            if (!Equals(identityKey, request.IdentityKey))
                return false;

            if (Guid.Empty.Equals(request.Identity))
                request.Identity = Guid.NewGuid();
            else
            {
                if (connectedServers.TryGetValue(request.Identity, out var exists))
                {
                    if (exists.GetState())
                        return false;

                    SignRoom(client, request);

                    connectedServers[request.Identity] = client;

                    client.ChangeOwner(exists);

                    return true;
                }
            }

            while (!connectedServers.TryAdd(request.Identity, client))
            {
                request.Identity = Guid.NewGuid();
            }

            SignRoom(client, request);

            return true;
        }

        private void SignRoom(RoomServerNetworkClient client, RoomSignInRequestModel request)
        {
            client.Id = request.Identity;

            client.ConnectionEndPoint = request.ConnectionEndPoint;

            client.Signed = true;
        }

        public CreateRoomSessionResponseModel CreateRoomSession(LobbyServerNetworkClient client, LobbyCreateRoomSessionRequestModel request)
        {
            var result = new CreateRoomSessionResponseModel();

            IEnumerable<RoomServerNetworkClient> servers = default;

            if (request.SpecialServer.HasValue)
            {
                if (connectedServers.TryGetValue(request.SpecialServer.Value, out var server))
                    servers = Enumerable.Repeat(server, 1);
            }
            else
            {
                IEnumerable<RoomServerNetworkClient> serverSelector = connectedServers.Values;

                if (request.Location != default)
                    serverSelector = serverSelector.Where(x=>request.Location.Equals(x.Location));

                servers = serverSelector.OrderBy(x => x.SessionsCount).Take(request.NeedPointCount);
            }

            servers = servers.Where(x => x.Network?.GetState() == true);

            result.Result = servers.Any();

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
