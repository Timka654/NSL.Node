using NSL.SocketClient;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Transport
{
    public class TransportNetworkClient : IServerNetworkClient
    {
        public Guid Id { get; set; }
    }
}
