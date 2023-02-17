using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.ConfigurationEngine;
using System.Collections.Concurrent;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("transport.server.port", 6998);

        public virtual string IdentityKey => Configuration.GetValue<string>("transport.server.identityKey", "AABBCC");

        protected NetworkListener Listener { get; private set; }

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

        public RoomServerEntry Run()
        {
            Listener = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddDisconnectHandle(Entry.RoomManager.OnDisconnectedRoomServer);

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeTransportPacketEnum.SignServerPID, SignServerReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeTransportPacketEnum.SignSessionPID, SignSessionReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeTransportPacketEnum.RoomStartupInfoPID, RoomStartupInfoReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeTransportPacketEnum.FinishRoom, RoomFinishRoomReceiveHandle);
                })
                .WithBindingPoint($"http://*:{BindingPort}/")
                .Build();

            Listener.Start();

            return this;
        }

        private void SignServerReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeTransportPacketEnum.SignServerResultPID);

            client.Id = data.ReadGuid();

            client.ConnectionEndPoint = data.ReadString16();

            var serverIdentityKey = data.ReadString16();


            if (!IdentityKey.Equals(serverIdentityKey))
            {
                response.WriteBool(false);

                client.Send(response);

                return;
            }

            bool result = Entry.RoomManager.TryRoomServerConnect(client);

            response.WriteBool(result);

            if (result)
                response.WriteGuid(client.Id);

            client.Send(response);
        }

        private void SignSessionReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeTransportPacketEnum.SignSessionResultPID);

            var identityKey = data.ReadString16();
            var id = data.ReadGuid();

            var session = client.GetSession(id);

            if (session == null || !session.Client.SessionIdentity.Equals(identityKey))
            {
                packet.WriteBool(false);

                client.Send(packet);

                return;
            }

            packet.WriteBool(true);

            packet.WriteString16(session.Client.LobbyServerIdentity);
            packet.WriteGuid(session.Client.RoomId);

            client.Send(packet);
        }

        private void RoomFinishRoomReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var lobbyServerIdentity = data.ReadString16();

            byte[] dataBuffer = data.Read(data.DataLength - data.DataPosition);

            var lobby = Entry.LobbyManager.GetLobbyById(lobbyServerIdentity);

            if (lobby == null)
                return; // todo

            var packet = OutputPacketBuffer.Create(NodeBridgeLobbyPacketEnum.FinishRoom);

            packet.Write(dataBuffer);

            lobby.Network.Send(packet);

        }
        private async void RoomStartupInfoReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeTransportPacketEnum.RoomStartupInfoResultPID);

            var lobbyServerIdentity = data.ReadString16();
            var roomId = data.ReadGuid();

            var result = await Entry.LobbyManager.GetRoomStartupInfo(lobbyServerIdentity, roomId);

            packet.WriteBool(result.Item1);

            if (result.Item1)
                packet.Write(result.Item2);

            client.Send(packet);
        }

        public record CreateSignResult(string endPoint, Guid id);
    }
}
