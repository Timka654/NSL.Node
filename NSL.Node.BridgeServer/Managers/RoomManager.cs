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
