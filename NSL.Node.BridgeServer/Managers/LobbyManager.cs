using NSL.Logger.Interface;
using NSL.Node.BridgeServer.CS;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Shared.Enums;
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

        public bool TryLobbyServerConnect(LobbyServerNetworkClient client, string identityKey)
        {
            //todo: add validation by identityKey

            connectedServers.Remove(client.Identity, out _);

            connectedServers.TryAdd(client.Identity, client);

            client.Signed = true;

            return true;
        }

        public LobbyServerNetworkClient? GetLobbyById(string lobbyId)
        {
            connectedServers.TryGetValue(lobbyId, out var lobby);

            return lobby;
        }

        public async Task<bool> ValidateClientSession(ClientServerNetworkClient client)
        {
            client.LobbyServer = GetLobbyById(client.LobbyServerIdentity);

            if (client.LobbyServer == null)
            {
                LogNotFoundLobbyByIdentity(nameof(ValidateClientSession), client.LobbyServerIdentity);

                return false;
            }

            bool result = default;

            var packet = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.ValidateSessionRequest);

            packet.WriteGuid(client.RoomId);
            packet.WriteString16(client.SessionIdentity);

            await client.LobbyServer.RequestBuffer.SendRequestAsync(packet, data =>
            {
                if (data != default)
                    result = data.ReadBool();

                return Task.CompletedTask;
            });

            logger.ConsoleLog(LoggerLevel.Info, $"[{nameof(ValidateClientSession)}] >>>>> result : {result}");

            return result;
        }

        public async Task<(bool result, byte[] data)> GetRoomStartupInfo(string lobbyServerIdentity, Guid roomId)
        {
            var server = GetLobbyById(lobbyServerIdentity);

            if (server == default)
            {
                LogNotFoundLobbyByIdentity(nameof(GetRoomStartupInfo), lobbyServerIdentity);

                return (false, default);
            }
            bool result = default;

            byte[] bytesData = default;

            var packet = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.RoomStartupInfoRequest);

            packet.WriteGuid(roomId);

            await server.RequestBuffer.SendRequestAsync(packet, data =>
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

        internal void SendLobbyFinishRoom(string lobbyServerIdentity, byte[] dataBuffer)
        {
            var lobby = GetLobbyById(lobbyServerIdentity);

            if (lobby == null)
            {
                LogNotFoundLobbyByIdentity(nameof(SendLobbyFinishRoom), lobbyServerIdentity);

                return; // todo
            }


            var packet = OutputPacketBuffer.Create(NodeBridgeLobbyPacketEnum.FinishRoomMessage);

            packet.Write(dataBuffer);

            lobby.Network.Send(packet);
        }

        internal void SendLobbyRoomMessage(string lobbyServerIdentity, byte[] dataBuffer)
        {
            var lobby = GetLobbyById(lobbyServerIdentity);

            if (lobby == null)
            {
                LogNotFoundLobbyByIdentity(nameof(SendLobbyRoomMessage), lobbyServerIdentity);

                return; // todo
            }

            var packet = OutputPacketBuffer.Create(NodeBridgeLobbyPacketEnum.RoomMessage);

            packet.Write(dataBuffer);

            lobby.Network.Send(packet);
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
