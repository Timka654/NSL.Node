﻿using NSL.BuilderExtensions.SocketCore;
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
        protected LobbyServerEntry LobbyServer { get; }

        protected RoomServerEntry TransportServer { get; }

        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("client.server.port", 7000);

        protected NetworkListener Listener { get; private set; }

        protected ILogger Logger { get; }

        protected BridgeServerEntry Entry { get; }

        public static ClientServerEntry Create(BridgeServerEntry entry, LobbyServerEntry lobbyServer, RoomServerEntry transportServer, string logPrefix = "[ClientServer]")
            => new ClientServerEntry(entry, lobbyServer, transportServer, logPrefix);

        public ClientServerEntry(BridgeServerEntry entry, LobbyServerEntry lobbyServer, RoomServerEntry transportServer, string logPrefix = "[ClientServer]")
        {
            Entry = entry;
            LobbyServer = lobbyServer;
            TransportServer = transportServer;

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
            client.ServerIdentity = data.ReadString16();
            client.RoomId = data.ReadGuid();
            client.SessionIdentity = data.ReadString16();

            var result = await LobbyServer.ValidateSession(client.ServerIdentity, client.RoomId, client.SessionIdentity);

            var packet = OutputPacketBuffer.Create(NodeBridgeClientPacketEnum.SignSessionResultPID);

            packet.WriteBool(result);

            if (result)
            {
                var sessions = TransportServer.CreateSignSession(client.SessionIdentity, client.RoomId);

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