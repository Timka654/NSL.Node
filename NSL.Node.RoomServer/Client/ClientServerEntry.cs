using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client.Data;
using NSL.WebSockets.Server;
using System;
using System.Collections.Concurrent;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        public string BindingAddress { get; }

        protected WSServerListener<TransportNetworkClient> network { get; private set; }

        public int ClientBindingPort => Entry.ClientBindingPort;

        protected RoomServerStartupEntry Entry { get; }

        protected ILogger Logger { get; }

        public static ClientServerEntry Create(
            RoomServerStartupEntry entry, 
            BridgeTransportNetwork bridgeNetwork,
            
            string logPrefix = "[ClientServer]")
            => new ClientServerEntry(entry, bridgeNetwork, logPrefix);

        public ClientServerEntry(RoomServerStartupEntry entry, BridgeTransportNetwork bridgeNetwork, string logPrefix = "[ClientServer]")
        {
            Entry = entry;

            this.bridgeNetwork = bridgeNetwork;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public ClientServerEntry Run()
        {
            network = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>()
                .WithBindingPoint($"http://*:{ClientBindingPort}/")
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddPacketHandle(
                        RoomPacketEnum.SignSession, SignInPacketHandle);
                    builder.AddPacketHandle(
                        RoomPacketEnum.Transport, TransportPacketHandle);
                    builder.AddPacketHandle(
                        RoomPacketEnum.Broadcast, BroadcastPacketHandle);
                    builder.AddPacketHandle(
                        RoomPacketEnum.ReadyNode, ReadyPacketHandle);
                    builder.AddPacketHandle(
                        RoomPacketEnum.Execute, ExecutePacketHandle);

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<TransportNetworkClient, WSServerOptions<TransportNetworkClient>>, TransportNetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);
                })
                .Build();

            network.Start();

            return this;
        }

        private readonly BridgeTransportNetwork bridgeNetwork;

        private ConcurrentDictionary<(string lobbyServerIdentity, Guid roomId), RoomInfo> roomMap = new ();
    }
}
