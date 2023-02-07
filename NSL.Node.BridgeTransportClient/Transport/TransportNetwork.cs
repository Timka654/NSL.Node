using NSL.BuilderExtensions.SocketCore;
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
    public partial class TransportNetwork<TRoomInfo>
        where TRoomInfo : RoomInfo, new()
    {
        public string BindingAddress { get; }

        protected WSServerListener<TransportNetworkClient<TRoomInfo>> network { get; private set; }

        public TransportNetwork(
            BridgeTransportNetwork bridgeNetwork,
            string bindingAddress,
            Action<WebSocketsServerEndPointBuilder<TransportNetworkClient<TRoomInfo>, WSServerOptions<TransportNetworkClient<TRoomInfo>>>> onBuild = null)
        {
            this.bridgeNetwork = bridgeNetwork;
            this.BindingAddress = bindingAddress;

            network = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient<TRoomInfo>>()
                .WithOptions<WSServerOptions<TransportNetworkClient<TRoomInfo>>>()
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
                        NodeTransportPacketEnum.ReadyNode, ReadyPacketHandle);
                    builder.AddPacketHandle(
                        NodeTransportPacketEnum.Execute, ExecutePacketHandle);

                    if (onBuild != null)
                        onBuild(builder);
                })
                .Build();

            network.Start();
        }

        private readonly BridgeTransportNetwork bridgeNetwork;

        private ConcurrentDictionary<Guid, TRoomInfo> roomMap = new ConcurrentDictionary<Guid, TRoomInfo>();
    }
}
