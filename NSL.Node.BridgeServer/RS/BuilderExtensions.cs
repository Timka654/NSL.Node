﻿using NSL.LocalBridge;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NSL.SocketCore.Utils;

namespace NSL.Node.BridgeServer.RS
{
    public static class BuilderExtensions
    {
        public static NodeBridgeServerEntryBuilder WithRoomServerLocalBridgeBinding<TAnotherClient>(
            this NodeBridgeServerEntryBuilder builder,
            out LocalBridgeClient<NetworkClient, TAnotherClient> bridge,
            string logPrefix = null)
            where TAnotherClient : INetworkClient, new()
        {
            var local = new RoomServerLocalBridgeEntry(builder.Entry, logPrefix);

            bridge = local.CreateLocalBridge<TAnotherClient>();

            return builder.WithRoomServerListener(local);
        }

        public static NodeBridgeServerEntryBuilder WithRoomServerBinding(this NodeBridgeServerEntryBuilder builder, int bindingPort,
            string logPrefix = null)
            => builder.WithRoomServerListener(new RoomServerEntry(builder.Entry, bindingPort, logPrefix));

        public static NodeBridgeServerEntryBuilder WithRoomServerBinding(this NodeBridgeServerEntryBuilder builder, string bindingPoint,
            string logPrefix = null)
            => builder.WithRoomServerListener(new RoomServerEntry(builder.Entry, bindingPoint, logPrefix));
    }
}
