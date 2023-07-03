using NSL.Logger.Interface;
using NSL.Logger;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeServer.RS.Packets;
using NSL.SocketServer.Utils;
using NSL.EndPointBuilder;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeServer.RS
{
    public abstract class RoomServerBaseEntry
    {
        protected INetworkListener Listener { get; set; }

        protected ILogger Logger { get; }

        protected NodeBridgeServerEntry Entry { get; }

        public RoomServerBaseEntry(NodeBridgeServerEntry entry, string logPrefix = null)
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix ?? "[TransportServer]");
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

            builder.AddDisconnectHandle(Entry.RoomManager.OnDisconnectedRoomServer);


            builder.AddDefaultEventHandlers<TBuilder, NetworkClient>(null,
                DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace & ~DefaultEventHandlersEnum.Receive & ~DefaultEventHandlersEnum.Send);

            builder.AddBaseSendHandle((client, pid, len, stack) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger.AppendInfo($"Send packet {pid}");
            });

            builder.AddBaseReceiveHandle((client, pid, len) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger.AppendInfo($"Receive packet {pid}");
            });

            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignServerRequest, SignServerPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionRequest, SignSessionPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.FinishRoomMessage, RoomFinishRoomPacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.RoomMessage, RoomMessagePacket.ReceiveHandle);
            builder.AddPacketHandle(NodeBridgeRoomPacketEnum.SignSessionPlayerRequest, SignSessionPlayerPacket.ReceiveHandle);

            return builder;
        }
    }
}
