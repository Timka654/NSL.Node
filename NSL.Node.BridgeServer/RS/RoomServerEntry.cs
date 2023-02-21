using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.ConfigurationEngine;
using NSL.Node.BridgeServer.RS.Packets;
using NSL.SocketServer.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.Node.BridgeServer.LS;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("transport_server_port", 6998);

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
            var server = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .AspWithOptions<NetworkClient, NetworkOptions>()
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddConnectHandle(client =>
                    {
                        if (client != null)
                            client.Entry = Entry;
                    });

                    builder.AddDisconnectHandle(Entry.RoomManager.OnDisconnectedRoomServer);

                    builder.AddDefaultEventHandlers<AspNetWebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignServerPID, SignServerPacket.ReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionPID, SignSessionPacket.ReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeRoomPacketEnum.RoomStartupInfoPID, RoomStartupInfoPacket.ReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeRoomPacketEnum.FinishRoom, RoomFinishRoomPacket.ReceiveHandle);
                }).BuildWithoutRoute();

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
            Listener = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddConnectHandle(client =>
                    {
                        if (client != null)
                            client.Entry = Entry;
                    });

                    builder.AddDisconnectHandle(Entry.RoomManager.OnDisconnectedRoomServer);

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignServerPID, SignServerPacket.ReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionPID, SignSessionPacket.ReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeRoomPacketEnum.RoomStartupInfoPID, RoomStartupInfoPacket.ReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeRoomPacketEnum.FinishRoom, RoomFinishRoomPacket.ReceiveHandle);
                })
                .WithBindingPoint($"http://*:{BindingPort}/")
                .Build();

            Listener.Start();

            return this;
        }
    }
}
