using NSL.BuilderExtensions.SocketCore;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.Node.RoomServer.Client.Data;
using System;
using System.Collections.Concurrent;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.EndPointBuilder;
using NSL.SocketServer.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Shared.Client.Core;

namespace NSL.Node.RoomServer.Client
{
    public abstract partial class ClientServerBaseEntry
    {
        protected INetworkListener Listener { get; set; }

        protected NodeRoomServerEntry Entry { get; }

        protected ILogger Logger { get; }

        public ClientServerBaseEntry(NodeRoomServerEntry entry, string logPrefix = null)
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix ?? "[ClientServer]");
        }

        public abstract void Run();

        public async Task<RoomInfo> TryLoadRoomAsync(Guid roomId, Guid sessionId)
        {
            if (!roomMap.TryGetValue(sessionId, out var roomInfo))
            {
                var result = await Entry.ValidateSession(new BridgeServer.Shared.Requests.RoomSignSessionRequestModel()
                {
                    SessionIdentity = sessionId,
                    RoomIdentity = roomId
                });

                if (result.Result == true)
                {
                    roomInfo = roomMap.GetOrAdd(sessionId, id => new Lazy<RoomInfo>(() =>
                    {
                        var room = new RoomInfo(Entry, sessionId, roomId);

                        room.OnRoomDisposed += () =>
                        {
                            roomMap.TryRemove(id, out _);
                        };

                        room.SetStartupInfo(new NodeRoomStartupInfo(result.Options));

                        return room;
                    }));
                }
            }

            return roomInfo?.Value;
        }

        protected TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<TransportNetworkClient>, IHandleIOBuilder
        {
            builder.SetLogger(Logger);

            builder.AddAsyncPacketHandle(
                RoomPacketEnum.SignSession, SignInPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.Transport, TransportPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.Broadcast, BroadcastPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.ReadyNodeRequest, ReadyPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.Execute, ExecutePacketHandle);

            builder.AddDefaultEventHandlers<TBuilder, TransportNetworkClient>(null,
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

            builder.AddDisconnectHandle(client =>
            {

                if (client.Room != null)
                    client.Room.OnClientDisconnected(client);
            });

            return builder;
        }

        private ConcurrentDictionary<Guid, Lazy<RoomInfo>> roomMap = new();
    }
}
