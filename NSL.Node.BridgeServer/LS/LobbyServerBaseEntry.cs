using NSL.BuilderExtensions.SocketCore;
using NSL.EndPointBuilder;
using NSL.Logger;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketServer.Utils;
using NSL.Logger.Interface;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NSL.SocketCore.Utils.Buffer;
using System;
using NSL.SocketCore.Utils.Logger;

namespace NSL.Node.BridgeServer.LS
{
    public abstract partial class LobbyServerBaseEntry
    {
        protected INetworkListener Listener { get; set; }

        protected IBasicLogger Logger { get; }

        protected NodeBridgeServerEntry Entry { get; }

        public LobbyServerBaseEntry(NodeBridgeServerEntry entry, string logPrefix = null)
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix ?? "[LobbyServer]");
        }

        public abstract void Run();

        protected TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<NetworkClient>, IHandleIOBuilder<NetworkClient>
        {
            builder.SetLogger(Logger);

            builder.AddConnectHandle(client =>
            {
                if (client != null)
                    client.Entry = Entry;
            });

            builder.AddDisconnectHandle(Entry.LobbyManager.OnDisconnectedLobbyServer);

            if (Logger != null)
                builder.AddDefaultEventHandlers((string)null,
                    DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace & ~DefaultEventHandlersEnum.Receive & ~DefaultEventHandlersEnum.Send);

            builder.AddSendHandle((client, pid, len, stack) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger?.AppendInfo($"Send packet {pid}({Enum.GetName((NodeBridgeLobbyPacketEnum)pid)})");
            });

            builder.AddReceiveHandle((client, pid, len) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger?.AppendInfo($"Receive packet {pid}({Enum.GetName((NodeBridgeLobbyPacketEnum)pid)})");
            });

            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.SignServerRequest, SignSessionRequestReceiveHandle);
            builder.AddAsyncPacketHandle(NodeBridgeLobbyPacketEnum.CreateRoomSessionRequest, CreateRoomSessionRequestReceiveHandle);
            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.AddPlayerRequest, AddPlayerRequestReceiveHandle);
            builder.AddPacketHandle(NodeBridgeLobbyPacketEnum.RemovePlayerRequest, RemovePlayerRequestReceiveHandle);

            builder.AddResponsePacketHandle(
                NodeBridgeLobbyPacketEnum.Response,
                client => client.RequestBuffer);

            return builder;
        }
    }
}
