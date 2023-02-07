using NSL.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeTransportClient.Shared
{
    public class PlayerInfo
    {
        public INetworkClient Network { get; set; }

        public Guid Id { get; set; }
    }
}
