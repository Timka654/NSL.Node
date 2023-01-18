﻿using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeTransportClient.Bridge;
using NSL.Node.BridgeTransportClient.Transport.Data;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using NSL.WebSockets.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Transport
{
    public partial class TransportNetwork
    {
        public string BindingAddress { get; }

        protected WSServerListener<TransportNetworkClient> network { get; private set; }

        public TransportNetwork(
            BridgeTransportNetwork bridgeNetwork,
            string bindingAddress,
            Action<WebSocketsServerEndPointBuilder<TransportNetworkClient, WSServerOptions<TransportNetworkClient>>> onBuild = null)
        {
            this.bridgeNetwork = bridgeNetwork;
            this.BindingAddress = bindingAddress;

            network = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>()
                .WithBindingPoint(bindingAddress)
                .WithCode(builder =>
                {
                    builder.AddPacketHandle(
                        NodeTransportPacketEnum.SignSession, SignInPacketHandle);
                    builder.AddPacketHandle(
                        NodeTransportPacketEnum.Transport, TransportPacketHandle);
                    builder.AddPacketHandle(
                        NodeTransportPacketEnum.Broadcast, BroadcastPacketHandle);
                    builder.AddPacketHandle(
                        NodeTransportPacketEnum.ReadyNodePID, ReadyPacketHandle);

                    if (onBuild != null)
                        onBuild(builder);
                })
                .Build();

            network.Start();
        }

        private readonly BridgeTransportNetwork bridgeNetwork;

        private ConcurrentDictionary<Guid, RoomInfo> roomMap = new ConcurrentDictionary<Guid, RoomInfo>();
    }
}
