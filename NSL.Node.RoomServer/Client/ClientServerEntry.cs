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
using NSL.Utils;
using NSL.EndPointBuilder;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using NSL.SocketServer.Utils;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        protected INetworkListener Listener { get; private set; }

        public int ClientBindingPort => Entry.Configuration.GetValue<int>("client_binding_port", 5920);

        public string ClientBindingPoint => Entry.Configuration.GetValue("client_binding_point", default(string));

        protected RoomServerStartupEntry Entry { get; }

        protected ILogger Logger { get; }

        public static ClientServerEntry Create(
            RoomServerStartupEntry entry,
            BridgeRoomNetwork bridgeNetwork,

            string logPrefix = "[ClientServer]")
            => new ClientServerEntry(entry, bridgeNetwork, logPrefix);

        public ClientServerEntry(RoomServerStartupEntry entry, BridgeRoomNetwork bridgeNetwork, string logPrefix = "[ClientServer]")
        {
            Entry = entry;

            this.bridgeNetwork = bridgeNetwork;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public ClientServerEntry RunAsp(IEndpointRouteBuilder builder, string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConvertionBuilder = null)
        {
            var server = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .AspWithOptions<TransportNetworkClient, WSServerOptions<TransportNetworkClient>>())
                .BuildWithoutRoute();

            var acceptDelegate = server.GetAcceptDelegate();

            var convBuilder = builder.MapGet(pattern, async context =>
            {
                if (requestHandle != null)
                    if (!await requestHandle(context))
                        return;

                await acceptDelegate(context);
            });

            if (actionConvertionBuilder != null)
                actionConvertionBuilder(convBuilder);


            Listener = server;

            return this;
        }

        public ClientServerEntry Run()
        {
            string bindingPoint = ClientBindingPoint;

            if (bindingPoint == default)
                bindingPoint = $"http://*:{ClientBindingPort}/";

            Listener = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>()
                .WithBindingPoint(bindingPoint))
                .Build();

            Listener.Start();

            return this;
        }

        private TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<TransportNetworkClient>, IHandleIOBuilder
        {
            builder.SetLogger(Logger);

            builder.AddAsyncPacketHandle(
                RoomPacketEnum.SignSession, SignInPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.Transport, TransportPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.Broadcast, BroadcastPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.ReadyNodeRequest, ReadyPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.Execute, ExecutePacketHandle);

            builder.AddDefaultEventHandlers<TBuilder, TransportNetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

            return builder;
        }

        private readonly BridgeRoomNetwork bridgeNetwork;

        private ConcurrentDictionary<(string lobbyServerIdentity, Guid roomId), RoomInfo> roomMap = new();
    }
}
