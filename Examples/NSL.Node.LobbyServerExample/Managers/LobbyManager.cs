﻿using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.Node.BridgeLobbyClient;
using NSL.Node.LobbyServerExample.Shared.Enums;
using NSL.Node.LobbyServerExample.Shared.Models;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Extensions.Packet;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Server;
using NSL.WebSockets.Server.AspNetPoint;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NSL.Node.LobbyServerExample.Managers
{
    public class LobbyManager
    {
        private readonly IConfiguration configuration;

        private ConcurrentDictionary<Guid, LobbyNetworkClientModel> clientMap = new ConcurrentDictionary<Guid, LobbyNetworkClientModel>();

        private ConcurrentDictionary<Guid, LobbyRoomInfoModel> roomMap = new ConcurrentDictionary<Guid, LobbyRoomInfoModel>();

        private ConcurrentDictionary<Guid, LobbyRoomInfoModel> processingRoomMap = new ConcurrentDictionary<Guid, LobbyRoomInfoModel>();

        internal void BuildNetwork(AspNetWebSocketsServerEndPointBuilder<LobbyNetworkClientModel, WSServerOptions<LobbyNetworkClientModel>> builder)
        {
            builder.AddConnectHandle(OnClientConnectedHandle);

            builder.AddDisconnectHandle(OnClientDisconnectedHandle);

            builder.AddPacketHandle(ServerReceivePacketEnum.CreateRoom, CreateRoomRequestHandle);
            builder.AddPacketHandle(ServerReceivePacketEnum.JoinRoom, JoinRoomRequestHandle);
            builder.AddPacketHandle(ServerReceivePacketEnum.LeaveRoom, LeaveRoomRequestHandle);
            builder.AddPacketHandle(ServerReceivePacketEnum.SendChatMessage, SendChatMessageRequestHandle);
            builder.AddPacketHandle(ServerReceivePacketEnum.StartRoom, RunRoomRequestHandle);
            builder.AddPacketHandle(ServerReceivePacketEnum.RemoveRoom, RemoveRoomRequestHandle);
            builder.AddPacketHandle(ServerReceivePacketEnum.GetRoomList, GetRoomListRequestHandle);
        }

        public LobbyManager(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #region NetworkHandle

        private void OnClientConnectedHandle(LobbyNetworkClientModel client)
        {
            do
            {
                client.UID = Guid.NewGuid();
            } while (!clientMap.TryAdd(client.UID, client));

            client.UID = client.UID;

            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.NewUserIdentity);

            packet.WriteGuid(client.UID);

            client.Network.Send(packet);
        }

        private void OnClientDisconnectedHandle(LobbyNetworkClientModel client)
        {
            var uid = client?.UID;

            if (uid != default)
            {
                clientMap.Remove(uid.Value, out _);

                if (client.CurrentRoom != default && client.CurrentRoom.State == LobbyRoomState.Lobby)
                    client.CurrentRoom.LeaveMember(client);
            }
        }

        #endregion

        internal Task<bool> BridgeValidateSessionAsync(string sessionIdentity)
        {
            var splited = sessionIdentity.Split(':');

            var roomId = Guid.Parse(splited.First());

            var uid = Guid.Parse(splited.Last());

            if (processingRoomMap.TryGetValue(roomId, out var room))
            {
                if (room.ExistsMember(uid))
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        #region PacketHandle

        private void GetRoomListRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.GetRoomListResult).WithWaitableAnswer(data);

            packet.WriteCollection(roomMap, item =>
            {
                packet.WriteGuid(item.Value.Id);

                packet.WriteString16(item.Value.Name);
            });

            client.Network.Send(packet);
        }

        private void CreateRoomRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.CreateRoomResult)
                .WithWaitableAnswer(data);

            LobbyRoomInfoModel room = data.ReadJson16<LobbyRoomInfoModel>();

            bool result = true; // if need

            room.OwnerId = client.UID;

            Guid rid;

            do
            {
                rid = Guid.NewGuid();
            } while (!roomMap.TryAdd(rid, room));

            room.Id = rid;

            room.JoinMember(client);

            packet.WriteBool(result);

            if (result)
            {
                packet.WriteGuid(rid);
            }

            client.Network.Send(packet);

            BroadcastNewLobbyRoom(room);
        }

        private void RemoveRoomRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            var room = client.CurrentRoom;

            if (room == null || room.State != LobbyRoomState.Lobby)
                return;

            if (client.UID == room.OwnerId) // only owner can remove room
            {
                if (roomMap.Remove(room.Id, out room))
                {
                    room.RemoveRoom();

                    BroadcastRemoveLobbyRoom(room);
                }
            }
        }

        private void RunRoomRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            var room = client.CurrentRoom;

            if (client.CurrentRoom == null || room.State != LobbyRoomState.Lobby)
                return;

            if (client.UID == room.OwnerId) // only owner can run game
            {

                if (roomMap.TryRemove(room.Id, out _))
                {
                    room.StartRoom(configuration);

                    processingRoomMap.TryAdd(room.Id, room);

                    BroadcastRemoveLobbyRoom(room);
                }
            }
        }

        private void SendChatMessageRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            if (client.CurrentRoom == null)
                return;

            client.CurrentRoom.SendChatMessage(client, data.ReadString16());
        }

        private void JoinRoomRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.JoinRoomResult);

            var rid = data.ReadGuid();

            var password = data.ReadString16();

            if (roomMap.TryGetValue(rid, out var room))
            {
                if (room.Password != default && !room.Password.Equals(password))
                    packet.WriteByte((byte)JoinResultEnum.InvalidPassword);
                else
                {
                    var joinResult = room.JoinMember(client);
                    packet.WriteByte((byte)joinResult);
                    if (joinResult == JoinResultEnum.Ok)
                    {
                        packet.WriteGuid(room.Id);
                        packet.WriteString16(room.Name);
                        packet.WriteGuid(room.OwnerId);

                        packet.WriteCollection(room.GetMembers(), item => packet.WriteGuid(item.Client.UID));

                        BroadcastChangeLobbyRoom(room);
                    }
                }
            }
            else
                packet.WriteByte((byte)JoinResultEnum.NotFound);

            client.Network.Send(packet);
        }

        private void LeaveRoomRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.LeaveRoomResult);

            packet.WriteBool(true);

            if (client.CurrentRoom != null)
            {
                var room = client.CurrentRoom;

                room.LeaveMember(client);

                BroadcastChangeLobbyRoom(room);
            }

            client.Network.Send(packet);
        }

        #endregion

        #region Broadcast

        private void BroadcastNewLobbyRoom(LobbyRoomInfoModel room)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.NewRoomMessage);

            packet.WriteJson16(new
            {
                room.Id,
                room.OwnerId,
                room.Name,
                room.MaxMembers,
                MemberCount = room.MemberCount(),
                room.PasswordEnabled
            });

            Broadcast(packet);
        }

        private void BroadcastRemoveLobbyRoom(LobbyRoomInfoModel room)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.NewRoomMessage);

            packet.WriteGuid(room.Id);

            Broadcast(packet);
        }

        private void BroadcastChangeLobbyRoom(LobbyRoomInfoModel room)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.NewRoomMessage);

            packet.WriteGuid(room.Id);
            packet.WriteInt32(room.MaxMembers);
            packet.WriteInt32(room.MemberCount());

            Broadcast(packet);
        }

        private async void Broadcast(OutputPacketBuffer buffer)
        {
            await Task.Run(() =>
            {
                foreach (var item in clientMap)
                {
                    item.Value.Network.Send(buffer, false);
                }

                buffer.Dispose();
            });
        }

        #endregion
    }
}
