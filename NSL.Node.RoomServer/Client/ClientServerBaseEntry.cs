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
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketServer;
using NSL.Extensions.Session.Server;
using NSL.SocketCore.Utils.Logger;

namespace NSL.Node.RoomServer.Client
{
    public abstract partial class ClientServerBaseEntry
    {
        protected INetworkListener Listener { get; set; }

        protected NodeRoomServerEntry Entry { get; }

        protected IBasicLogger Logger { get; }

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

        protected NSLSessionManager<TransportNetworkClient> sessionManager;

        protected TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<TransportNetworkClient>, IHandleIOBuilder
        {
            builder.AddConnectHandle(client => client.InitializeObjectBag());

            var options = builder.GetCoreOptions() as ServerOptions<TransportNetworkClient>;

            sessionManager = options.AddNSLSessions(c =>
            {
                c.CloseSessionDelay = TimeSpan.FromSeconds(3);
                c.OnRecoverySession += (client, session) =>
                {
                    var sSession = session as NSLServerSessionInfo<TransportNetworkClient>;

                    options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Try recovery session {sSession.Session} from {client.Network?.GetRemotePoint()}");

                    client.ChangeOwner(sSession.Client);

                    if (client.Room == null)
                    {
                        Task.Delay(1000).ContinueWith((t) =>
                        {
                            client.Network?.Disconnect();
                        });
                    }
                };

                c.OnExpiredSession += (network, session) =>
                {
                    var sSession = session as NSLServerSessionInfo<TransportNetworkClient>;

                    options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Session expired {sSession.Session}");

                    sSession.Client.Room?.DisconnectClient(sSession.Client);
                };
            });

            builder.SetLogger(Logger);

            builder.AddAsyncPacketHandle(
                RoomPacketEnum.SignSessionRequest, SignInPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.TransportMessage, TransportPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.BroadcastMessage, BroadcastPacketHandle);
            builder.AddAsyncPacketHandle(
                RoomPacketEnum.ReadyNodeRequest, ReadyPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.ExecuteMessage, ExecutePacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.DisconnectMessage, DisconnectMessagePacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.NodeChangeEndPointMessage, ChangeConnectionPointPacketHandle);

            builder.AddDefaultEventHandlers<TBuilder, TransportNetworkClient>(null,
                DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace & ~DefaultEventHandlersEnum.Receive & ~DefaultEventHandlersEnum.Send);

            builder.AddBaseSendHandle((client, pid, len, stack) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger.AppendInfo($"Send packet {pid}({Enum.GetName((RoomPacketEnum)pid)})");
            });

            builder.AddBaseReceiveHandle((client, pid, len) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger.AppendInfo($"Receive packet {pid}({Enum.GetName((RoomPacketEnum)pid)})");
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
