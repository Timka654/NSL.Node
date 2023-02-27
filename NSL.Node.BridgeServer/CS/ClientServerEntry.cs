using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger;
using NSL.Logger.Interface;

using NetworkClient = NSL.Node.BridgeServer.CS.ClientServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.CS.ClientServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.CS.ClientServerNetworkClient>;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.ConfigurationEngine;
using NSL.Node.BridgeServer.CS.Packets;
using Microsoft.AspNetCore.Routing;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.WebSockets.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using NSL.SocketServer.Utils;

namespace NSL.Node.BridgeServer.CS
{
    public class ClientServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("client_server_port", 7000);

        public string BindingPoint => Entry.Configuration.GetValue("client_server_point", default(string));

        protected INetworkListener Listener { get; private set; }

        protected ILogger Logger { get; }

        protected BridgeServerStartupEntry Entry { get; }

        public static ClientServerEntry Create(BridgeServerStartupEntry entry, string logPrefix = "[ClientServer]")
            => new ClientServerEntry(entry, logPrefix);

        public ClientServerEntry(BridgeServerStartupEntry entry, string logPrefix = "[ClientServer]")
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public ClientServerEntry RunAsp(IEndpointRouteBuilder builder, string pattern,
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

                    builder.AddDefaultEventHandlers<AspNetWebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeClientPacketEnum.SignSessionPID, SignSessionPacket.ReceiveHandle);
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

        public ClientServerEntry Run()
        {
            string bindingPoint = BindingPoint;

            if (bindingPoint == default)
                bindingPoint = $"http://*:{BindingPort}/";

            Listener = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithBindingPoint(bindingPoint)
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddConnectHandle(client =>
                    {
                        if (client != null)
                            client.Entry = Entry;
                    });

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeClientPacketEnum.SignSessionPID, SignSessionPacket.ReceiveHandle);
                })
                .Build();

            Listener.Start();

            return this;
        }
    }
}
