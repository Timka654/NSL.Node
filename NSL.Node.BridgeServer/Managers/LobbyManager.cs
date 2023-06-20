using NSL.Logger.Interface;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.RS;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer.Managers
{
    internal class LobbyManager
    {
        private readonly BridgeServerStartupEntry entry;

        private ILogger logger => entry.Logger;

        public LobbyManager(BridgeServerStartupEntry entry)
        {
            this.entry = entry;
        }

        public void OnDisconnectedLobbyServer(LobbyServerNetworkClient client)
        {
            if (client.Signed)
                connectedServers.Remove(client.Identity, out _);
        }

        public bool TryLobbyServerConnect(LobbyServerNetworkClient client, LobbySignInRequestModel request)
        {
            //todo: add validation by identityKey

            if(connectedServers.TryGetValue(client.Identity, out var oldClient))
            {
                if (oldClient.Network?.GetState() == true)
                {
                    return false;
                }

                client.LoadFrom(oldClient);

                connectedServers.TryRemove(client.Identity, out _);
            }

            connectedServers.TryAdd(client.Identity, client);

            client.Signed = true;

            return true;
        }

        public LobbyServerNetworkClient? GetLobbyById(string lobbyId)
        {
            connectedServers.TryGetValue(lobbyId, out var lobby);

            return lobby;
        }


        private ConcurrentDictionary<string, LobbyServerNetworkClient> connectedServers = new ConcurrentDictionary<string, LobbyServerNetworkClient>();


        #region helpers

        private void LogNotFoundLobbyByIdentity(string methodName, string identity)
        {
            logger.ConsoleLog(LoggerLevel.Error, $"[{methodName}] not found lobby by id {identity}");
        }

        #endregion

    }
}
