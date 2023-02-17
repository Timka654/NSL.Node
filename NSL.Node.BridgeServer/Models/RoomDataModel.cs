using NSL.Node.BridgeServer.CS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer.Models
{
    internal class RoomDataModel
    {
        public Guid RoomId { get; set; }

        public ConcurrentDictionary<string, ClientServerNetworkClient> Clients { get; } = new ConcurrentDictionary<string, ClientServerNetworkClient>();
    }
}
