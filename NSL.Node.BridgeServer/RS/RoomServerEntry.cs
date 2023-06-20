using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.ConfigurationEngine;
using NSL.Node.BridgeServer.RS.Packets;
using NSL.SocketServer.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.EndPointBuilder;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("room_server_port", transportBindingPort);

        [Obsolete]
        public virtual int transportBindingPort => Configuration.GetValue("transport_server_port", 6998);

        public virtual string BindingPoint => Configuration.GetValue("room_server_point", default(string));

        protected INetworkListener Listener { get; private set; }

        protected ILogger Logger { get; }

        protected BridgeServerStartupEntry Entry { get; }

        public static RoomServerEntry Create(BridgeServerStartupEntry entry, string logPrefix = "[TransportServer]")
            => new RoomServerEntry(entry, logPrefix);

        public RoomServerEntry(BridgeServerStartupEntry entry, string logPrefix = "[TransportServer]")
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public RoomServerEntry RunAsp(IEndpointRouteBuilder builder, string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConvertionBuilder = null)
        {
            var server = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .AspWithOptions<NetworkClient, NetworkOptions>())
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

        public RoomServerEntry Run()
        {
            string bindingPoint = BindingPoint;

            if (bindingPoint == default)
                bindingPoint = $"http://*:{BindingPort}/";

            Listener = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithBindingPoint(bindingPoint))
                .Build();

            Listener.Start();

            return this;
        }

        private TBuilder Fill<TBuilder>(TBuilder builder)
            //where TBuilder : WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>
            where TBuilder : IOptionableEndPointBuilder<NetworkClient>, IHandleIOBuilder
        {
            builder.SetLogger(Logger);

            builder.AddConnectHandle(client =>
            {
                if (client != null)
                    client.Entry = Entry;
            });

            builder.AddDisconnectHandle(Entry.RoomManager.OnDisconnectedRoomServer);


            builder.AddDefaultEventHandlers<TBuilder, NetworkClient>(null,
                DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace & ~DefaultEventHandlersEnum.Receive & ~DefaultEventHandlersEnum.Send);

            builder.AddBaseSendHandle((client, pid, len, stack) =>
            {
                if (pid < ushort.MaxValue - 100)
                    Logger.AppendInfo($"Send packet {pid}");
            });

            builder.AddBaseReceiveHandle((client, pid, len) =>
            {
                if (pid < ushort.MaxValue - 100)
                    Logger.AppendInfo($"Receive packet {pid}");
            });

            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignServerRequest, SignServerPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionRequest, SignSessionPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.FinishRoomMessage, RoomFinishRoomPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.RoomMessage, RoomMessagePacket.ReceiveHandle);

            return builder;
        }
    }
}
