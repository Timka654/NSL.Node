using NSL.Node.BridgeServer.CS;
using NSL.Node.BridgeServer.RS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Node.BridgeServer.Managers
{
    internal class RoomManager
    {
        private readonly BridgeServerStartupEntry entry;

        private BridgeConfigurationManager Configuration => entry.Configuration;

        public virtual int TransportServerCountPerRoom => Configuration.GetValue("transport_server_count_perRoom", 1);

        public RoomManager(BridgeServerStartupEntry entry)
        {
            this.entry = entry;
        }

        public void OnDisconnectedRoomServer(RoomServerNetworkClient client)
        {
            if (client.Signed)
                connectedServers.Remove(client.Id, out _);
        }

        internal void CreateRoom(ClientServerNetworkClient client)
        {
            var lobby = client.LobbyServer;

            client.Room = lobby.Rooms.GetOrAdd(client.RoomId, roomId => new Models.RoomDataModel() { RoomId = client.RoomId });

            client.Room.Clients.TryAdd(client.SessionIdentity, client);
        }

        internal List<CreateSignResult> CreateSignSession(ClientServerNetworkClient client)
        {
            List<CreateSignResult> result = new List<CreateSignResult>();

            var serverArray = connectedServers.Values.ToArray();

            var offset = client.RoomId.GetHashCode() % serverArray.Length;

            var room = client.Room;

            // single server for now - maybe change to multiple later

            var selectedServers = serverArray.Skip(offset).Take(TransportServerCountPerRoom).ToList();

            var missedCount = TransportServerCountPerRoom - selectedServers.Count;

            if (missedCount > 0)
            {
                selectedServers.AddRange(serverArray.Take(missedCount).Where(x => !selectedServers.Contains(x)));
            }

            if (selectedServers.Any())
            {
                var newId = Guid.NewGuid();

                if (client.TransportSessions != default)
                {
                    foreach (var item in client.TransportSessions)
                    {
                        item.Dispose();
                    }
                }

                client.TransportSessions = new TransportSession[selectedServers.Count];

                int i = 0;

                foreach (var server in selectedServers)
                {
                    var tsession = new TransportSession(client) { TransportIdentity = newId };

                    while (!server.TryAddSession(tsession))
                    {
                        newId = tsession.TransportIdentity = Guid.NewGuid();
                    }

                    client.TransportSessions[i] = tsession;

                    result.Add(new CreateSignResult(server.ConnectionEndPoint, newId));

                    i++;
                }
            }

            return result;
        }

        internal bool TryRoomServerConnect(RoomServerNetworkClient client)
        {
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

        private ConcurrentDictionary<Guid, RoomServerNetworkClient> connectedServers = new ConcurrentDictionary<Guid, RoomServerNetworkClient>();
    }

    public record CreateSignResult(string endPoint, Guid id);
}
