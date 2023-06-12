using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.RS;
using System;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.Models
{
    public class BridgeRoomInfoModel
    {
        public LobbyServerNetworkClient OwnerLobbyServer { get; set; }

        public ConcurrentDictionary<Guid, BridgeRoomEndPointModel> RoomPoints { get; set; } = new ConcurrentDictionary<Guid, BridgeRoomEndPointModel>();
    }

    public class BridgeRoomEndPointModel
    {
        public Guid RoomId { get; set; }

        public RoomServerEntry OwnerRoomServer { get; set; }
    }
}
