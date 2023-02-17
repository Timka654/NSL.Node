using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.SocketCore.Utils.Buffer;

using NetworkClient = NSL.Node.BridgeServer.CS.ClientServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.CS.ClientServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.CS.ClientServerNetworkClient>;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.ConfigurationEngine;
using NSL.Node.BridgeServer.RS;

namespace NSL.Node.BridgeServer.CS
{
    public class ClientServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("client.server.port", 7000);

        protected NetworkListener Listener { get; private set; }

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

        public ClientServerEntry Run()
        {
            Listener = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeClientPacketEnum.SignSessionPID, SignSessionReceiveHandle);
                })
                .WithBindingPoint($"http://*:{BindingPort}/")
                .Build();

            Listener.Start();

            return this;
        }

        #region Handles

        private async void SignSessionReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            client.LobbyServerIdentity = data.ReadString16();
            client.RoomId = data.ReadGuid();
            client.SessionIdentity = data.ReadString16();

            client.Signed = await Entry.LobbyManager.ValidateClientSession(client);

            var packet = OutputPacketBuffer.Create(NodeBridgeClientPacketEnum.SignSessionResultPID);

            packet.WriteBool(client.Signed);

            if (client.Signed)
            {
                Entry.RoomManager.CreateRoom(client);

                var sessions = Entry.RoomManager.CreateSignSession(client);

                packet.WriteCollection(sessions, i =>
                {
                    packet.WriteString16(i.endPoint);
                    packet.WriteGuid(i.id);
                });
            }

            client.Network.Send(packet);
        }

        #endregion
    }
}
