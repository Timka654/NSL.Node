using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Server;
using SimpleGame;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using WelcomeServer.Data.Models;
using WelcomeServer.Data.Repositories.Interfaces;
using WelcomeServer.Enums;
using WelcomeServer.Managers.Interfaces;

namespace WelcomeServer.Managers
{
    public class LobbyManager
    {
        private readonly IConfiguration configuration;

        private ConcurrentDictionary<string, Guid> TemporaryCreds = new ConcurrentDictionary<string, Guid>();
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
            builder.AddPacketHandle(ServerReceivePacketEnum.Handshake, CheckAuthToSocket);
            //builder.AddPacketHandle(ServerReceivePacketEnum.PerformGameMove, UpdateGame);
        }

        public LobbyManager(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #region NetworkHandle

        private async void OnClientConnectedHandle(LobbyNetworkClientModel client)
        {
            OutputPacketBuffer packet;

            //var ip = client.Network.GetRemotePoint().Address;
            packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.NewUserIdentity);
            //packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.HandshakeRequest);
            packet.WriteGuid(client.UID);
            client.Network.Send(packet);
        }

        private void OnClientDisconnectedHandle(LobbyNetworkClientModel client)
        {
            var uid = client?.UID;

            if (uid != default)
            {
                clientMap.Remove(uid.Value, out _);

                LeaveRoomMember(client);
            }
        }

        #endregion

        internal Task<bool> BridgeValidateSessionAsync(Guid roomId, string sessionIdentity)
        {
            var splited = sessionIdentity.Split(':');

            var uid = Guid.Parse(splited.First());

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

        private void CheckAuthToSocket(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            var x = data.ReadGuid();
            if (!TemporaryCreds.Values.Contains(x))
            {
                var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.ErrorHandShake);
                packet.WriteString32("Handshake failed");
                client.Network.Send(packet);
                client.Network.Disconnect();
            }
            else
            {
                clientMap[x] = client;
                client.UID = x;
            }
            //var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.GetRoomListResult).WithWaitableAnswer(data);

            //packet.WriteCollection(roomMap, item =>
            //{
            //    packet.WriteGuid(item.Value.Id);

            //    packet.WriteString16(item.Value.Name);
            //});

            //client.Network.Send(packet);
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

                    StartGame(room);
                }
            }
        }

        private void StartGame(LobbyRoomInfoModel room)
        {
            var playersCount = room.MemberCount();
            var gameModel = new GameModel(playersCount);

            var gameInfoForPlayers = gameModel.GetMaskedInfo();
        }

        private void SendChatMessageRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            if (client.CurrentRoom == null)
                return;

            client.CurrentRoom.SendChatMessage(client, data.ReadString16());
        }

        private void JoinRoomRequestHandle(LobbyNetworkClientModel client, InputPacketBuffer data)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.JoinRoomResult).WithWaitableAnswer(data);

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
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.LeaveRoomResult).WithWaitableAnswer(data);

            packet.WriteBool(true);

            LeaveRoomMember(client);

            client.Network.Send(packet);
        }

        #endregion


        private void LeaveRoomMember(LobbyNetworkClientModel client)
        {
            var room = client.CurrentRoom;

            if (room != default && room.State == LobbyRoomState.Lobby)
            {
                // logic for destroy room if creator leave
                if (room.OwnerId.Equals(client.UID))
                {
                    roomMap.TryRemove(room.Id, out _);
                    room.LeaveMember(client);

                    BroadcastRemoveLobbyRoom(room);
                }
                else
                    room.LeaveMember(client);
            }
        }


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
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.RoomRemoveMessage);

            packet.WriteGuid(room.Id);

            Broadcast(packet);
        }

        private void BroadcastChangeLobbyRoom(LobbyRoomInfoModel room)
        {
            var packet = OutputPacketBuffer.Create(ClientReceivePacketEnum.ChangeTitleRoomInfo);

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

        public async Task<Guid> RegisterTemporaryKey(string username)
        {
            TemporaryCreds[username] = Guid.NewGuid();
            return TemporaryCreds[username];
        }

    }
}
