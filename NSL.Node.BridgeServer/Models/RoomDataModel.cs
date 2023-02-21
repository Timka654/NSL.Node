using NSL.Node.BridgeServer.CS;
using System;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.Models
{
    internal class RoomDataModel
    {
        public Guid RoomId { get; set; }

        public ConcurrentDictionary<string, ClientServerNetworkClient> Clients { get; } = new ConcurrentDictionary<string, ClientServerNetworkClient>();
    }
}
