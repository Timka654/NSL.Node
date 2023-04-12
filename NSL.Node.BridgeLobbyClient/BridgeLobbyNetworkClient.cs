﻿using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Node.BridgeLobbyClient
{
    public class BridgeLobbyNetworkClient : BaseSocketNetworkClient
    {
        public RequestProcessor PacketWaitBuffer { get; }

        public BridgeLobbyNetworkClient()
        {
            PacketWaitBuffer = new RequestProcessor(this);
        }

        public override void Dispose()
        {
            PacketWaitBuffer.Dispose();

            base.Dispose();
        }
    }
}
