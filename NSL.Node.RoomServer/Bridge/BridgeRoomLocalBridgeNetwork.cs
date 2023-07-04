﻿using NSL.BuilderExtensions.LocalBridge;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.LocalBridge;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Client;
using System;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Bridge
{
    public class BridgeRoomLocalBridgeNetwork<TServerClient> : BridgeRoomBaseNetwork
        where TServerClient : INetworkClient, new()
    {
        private LocalBridgeClient<TServerClient, BridgeRoomNetworkClient> serverNetwork;
        LocalBridgeClient<BridgeRoomNetworkClient, TServerClient> localNetwork;

        public BridgeRoomLocalBridgeNetwork(NodeRoomServerEntry entry, string publicEndPoint, string identityKey, Guid serverId = default, string logPrefix = null) : base(entry, publicEndPoint, identityKey, serverId, logPrefix)
        {
            var builder = FillOptions(WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeRoomNetworkClient>()
                .WithOptions<WSClientOptions<BridgeRoomNetworkClient>>());

            localNetwork = builder.CreateLocalBridge<BridgeRoomNetworkClient, TServerClient>();
        }

        public BridgeRoomLocalBridgeNetwork<TServerClient> WithServerClient(LocalBridgeClient<TServerClient, BridgeRoomNetworkClient> serverClient)
        {
            serverNetwork = serverClient;

            return this;
        }

        protected override Task<bool> InitNetwork()
        {
            localNetwork.SetOtherClient(serverNetwork);

            return Task.FromResult(true);
        }
    }
}