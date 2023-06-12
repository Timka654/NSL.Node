using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.ConfigurationEngine;
using NSL.Node.BridgeServer.LS.Packets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System;
using NSL.SocketServer.Utils;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.EndPointBuilder;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("lobby_server_port", 6999);

        public virtual string BindingPoint => Configuration.GetValue("lobby_server_point", default(string));

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

        public LobbyServerEntry Run()
        {
            string bindingPoint = BindingPoint;

            if (bindingPoint == default)
                bindingPoint = $"http://*:{BindingPort}/";

            Listener = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>())
                .WithBindingPoint(bindingPoint)
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

            builder.AddDisconnectHandle(Entry.LobbyManager.OnDisconnectedLobbyServer);


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

            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.SignServerRequest, SignSessionRequestPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.CreateRoomSessionRequest, CreateRoomSessionRequestPacket.ReceiveHandle);

            builder.AddReceivePacketHandle(
                NodeBridgeLobbyPacketEnum.Response,
                client => client.RequestBuffer);

            return builder;
        }
    }
}
