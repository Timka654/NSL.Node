using NSL.BuilderExtensions.SocketCore;
using NSL.EndPointBuilder;
using NSL.Logger;
using NSL.Node.BridgeServer.LS.Packets;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketServer.Utils;
using NSL.Logger.Interface;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeServer.LS
{
    public abstract class LobbyServerBaseEntry
    {
        protected INetworkListener Listener { get; set; }

        protected ILogger Logger { get; }

        protected NodeBridgeServerEntry Entry { get; }

        public LobbyServerBaseEntry(NodeBridgeServerEntry entry, string logPrefix = null)
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix ?? "[LobbyServer]");
        }

        public abstract void Run();

        protected TBuilder Fill<TBuilder>(TBuilder builder)
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

            if (Logger != null)
                builder.AddDefaultEventHandlers<TBuilder, NetworkClient>(null,
                    DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace & ~DefaultEventHandlersEnum.Receive & ~DefaultEventHandlersEnum.Send);

            builder.AddBaseSendHandle((client, pid, len, stack) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger?.AppendInfo($"Send packet {pid}");
            });

            builder.AddBaseReceiveHandle((client, pid, len) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger?.AppendInfo($"Receive packet {pid}");
            });

            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.SignServerRequest, SignSessionRequestPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.CreateRoomSessionRequest, CreateRoomSessionRequestPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.AddPlayerRequest, AddPlayerRequestPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.RemovePlayerRequest, RemovePlayerRequestPacket.ReceiveHandle);

            builder.AddReceivePacketHandle(
                NodeBridgeLobbyPacketEnum.Response,
                client => client.RequestBuffer);

            return builder;
        }
    }
}
