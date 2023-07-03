using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeServer.Shared.Message;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomSession : IDisposable
    {
        private AutoResetEvent playerIdsLocker = new(true);

        public event Action<RoomSession> OnDestroy = session => { };

        public Guid SessionId { get; set; }

        public Guid RoomIdentity { get; }

        public bool Active { get; set; }

        public LobbyServerNetworkClient OwnedLobbyNetwork { get; set; }

        public RoomServerNetworkClient OwnedRoomNetwork { get; set; }

        public NodeRoomStartupInfo StartupInfo { get; set; }

        public List<Guid>? PlayerIds { get; set; }

        public void AddPlayerId(Guid playerId)
        {
            playerIdsLocker.WaitOne();

            if (PlayerIds == default)
                PlayerIds = new List<Guid>();

            if (!PlayerIds.Contains(playerId))
                PlayerIds.Add(playerId);

            playerIdsLocker.Set();
        }

        public void RemovePlayerId(Guid playerId)
        {
            if (PlayerIds == default)
                return;

            playerIdsLocker.WaitOne();

            if (!PlayerIds.Contains(playerId))
                PlayerIds.Remove(playerId);

            playerIdsLocker.Set();
        }

        internal void SendLobbyFinishRoom(byte[] dataBuffer)
        {
            var packet = OutputPacketBuffer.Create(NodeBridgeLobbyPacketEnum.FinishRoomMessage);

            new RoomFinishMessageModel
            {
                SessionId = RoomIdentity,
                Data = dataBuffer
            }.WriteFullTo(packet);

            OwnedLobbyNetwork.Network?.Send(packet);
        }

        internal void SendLobbyRoomMessage(byte[] dataBuffer)
        {
            var packet = OutputPacketBuffer.Create(NodeBridgeLobbyPacketEnum.RoomMessage);

            new RoomMessageModel
            {
                SessionId = RoomIdentity,
                Data = dataBuffer
            }.WriteFullTo(packet);

            OwnedLobbyNetwork.Network?.Send(packet);
        }

        public RoomSession(Guid roomIdentity, LobbyServerNetworkClient ownedLobbyNetwork, RoomServerNetworkClient ownedRoomNetwork)
        {
            RoomIdentity = roomIdentity;
            OwnedLobbyNetwork = ownedLobbyNetwork;
            OwnedRoomNetwork = ownedRoomNetwork;

            // Destroy session timer
            new Timer(state => { if (!Active) Dispose(); }, default, TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);
        }

        public void Dispose()
        {
            OnDestroy(this);
        }

        internal bool ValidatePlayer(Guid playerId)
        {
            if (PlayerIds == default)
                return true;


            playerIdsLocker.WaitOne();

            bool result = PlayerIds.Contains(playerId);
             
            playerIdsLocker.Set();

            return result;
        }
    }
}
