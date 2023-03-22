using NSL.Node.BridgeServer.CS;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
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

        public LobbyManager(BridgeServerStartupEntry entry)
        {
            this.entry = entry;
        }

        public void OnDisconnectedLobbyServer(LobbyServerNetworkClient client)
        {
            if (client.Signed)
                connectedServers.Remove(client.Identity, out _);
        }

        public bool TryLobbyServerConnect(LobbyServerNetworkClient client, string identityKey)
        {
            connectedServers.Remove(client.Identity, out _);

            connectedServers.TryAdd(client.Identity, client);

            client.Signed = true;

            return true;
        }

        public LobbyServerNetworkClient GetLobbyById(string lobbyId)
        {
            connectedServers.TryGetValue(lobbyId, out var lobby);

            return lobby;
        }

        public async Task<bool> ValidateClientSession(ClientServerNetworkClient client)
        {
            client.LobbyServer = GetLobbyById(client.LobbyServerIdentity);

            if (client.LobbyServer == null)
            {
                entry.Logger.ConsoleLog(LoggerLevel.Info, " >>>>> lobby server is null");
                return false;
            }

            bool result = default;

            var packet = WaitablePacketBuffer.Create(NodeBridgeLobbyPacketEnum.ValidateSessionRequest);

            packet.WriteGuid(client.RoomId);
            packet.WriteString16(client.SessionIdentity);

            await client.LobbyServer.RequestBuffer.SendWaitRequest(packet, data =>
            {
                if (data != default)
                    result = data.ReadBool();
               
                return Task.CompletedTask;
            });

            entry.Logger.ConsoleLog(LoggerLevel.Info, $" >>>>> result : {result}");
            return result;
        }

        public async Task<(bool, byte[])> GetRoomStartupInfo(string serverIdentity, Guid roomId)
        {
            if (!connectedServers.TryGetValue(serverIdentity, out var server))
                return (false, default);

            bool result = default;

            byte[] bytesData = default;

            var packet = WaitablePacketBuffer.Create(NodeBridgeLobbyPacketEnum.RoomStartupInfoRequest);

            packet.WriteGuid(roomId);

            await server.RequestBuffer.SendWaitRequest(packet, data =>
            {
                if (data != default)
                {
                    result = data.ReadBool();

                    if (result)
                        bytesData = data.Read(data.DataLength - data.DataPosition);
                }
                return Task.CompletedTask;
            });

            return (result, bytesData);
        }


        private ConcurrentDictionary<string, LobbyServerNetworkClient> connectedServers = new ConcurrentDictionary<string, LobbyServerNetworkClient>();
    }
}
