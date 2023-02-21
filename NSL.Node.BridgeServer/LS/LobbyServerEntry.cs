using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.ConfigurationEngine;
using NSL.Node.BridgeServer.LS.Packets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.Node.BridgeServer.CS;
using System.Threading.Tasks;
using System;
using NSL.SocketServer.Utils;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("lobby_server_port", 6999);

        public virtual string IdentityKey => Configuration.GetValue("lobby_server_identityKey", "AABBCC");

        protected INetworkListener Listener { get; private set; }

        protected ILogger Logger { get; }

        protected BridgeServerStartupEntry Entry { get; }

        public static LobbyServerEntry Create(BridgeServerStartupEntry entry, string logPrefix = "[LobbyServer]")
            => new LobbyServerEntry(entry, logPrefix);

        public LobbyServerEntry(BridgeServerStartupEntry entry, string logPrefix = "[LobbyServer]")
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public LobbyServerEntry RunAsp(IEndpointRouteBuilder builder, string pattern,
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

                    builder.AddDisconnectHandle(Entry.LobbyManager.OnDisconnectedLobbyServer);

                    builder.AddDefaultEventHandlers<AspNetWebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.SignServerPID, SignSessionPacket.ReceiveHandle);

                    builder.AddReceivePacketHandle(
                        NodeBridgeLobbyPacketEnum.ValidateSessionResultPID,
                        client => client.RequestBuffer);

                    builder.AddReceivePacketHandle(
                        NodeBridgeLobbyPacketEnum.RoomStartupInfoResultPID,
                        client => client.RequestBuffer);
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

        public LobbyServerEntry Run()
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

                    builder.AddDisconnectHandle(Entry.LobbyManager.OnDisconnectedLobbyServer);

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.SignServerPID, SignSessionPacket.ReceiveHandle);

                    builder.AddReceivePacketHandle(
                        NodeBridgeLobbyPacketEnum.ValidateSessionResultPID,
                        client => client.RequestBuffer);

                    builder.AddReceivePacketHandle(
                        NodeBridgeLobbyPacketEnum.RoomStartupInfoResultPID,
                        client => client.RequestBuffer);
                })
                .WithBindingPoint($"http://*:{BindingPort}/")
                .Build();

            Listener.Start();

            return this;
        }
    }
}
