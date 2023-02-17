using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;
using System.Collections.Concurrent;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.ConfigurationEngine;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("lobby.server.port", 6999);

        public virtual string IdentityKey => Configuration.GetValue("lobby.server.identityKey", "AABBCC");

        protected NetworkListener Listener { get; private set; }

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

        public LobbyServerEntry Run()
        {
            Listener = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddDisconnectHandle(Entry.LobbyManager.OnDisconnectedLobbyServer);

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.SignServerPID, SignServerReceiveHandle);

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

        private void SignServerReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeLobbyPacketEnum.SignServerResultPID);

            client.Identity = data.ReadString16();

            bool result = Entry.LobbyManager.TryLobbyServerConnect(client, data.ReadString16());

            packet.WriteBool(result);

            client.Network.Send(packet);
        }
    }
}
