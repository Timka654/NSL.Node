using NSL.SocketClient;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Transport.Data
{
    public class TransportNetworkClient : IServerNetworkClient
    {
        public string Token { get; set; }

        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public RoomInfo Room { get; set; }

        public string EndPoint { get; set; }

        public bool Ready { get; set; }
    }
}
