using NSL.BuilderExtensions.SocketCore;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.Node.P2Proxy.Client.Data;
using System;
using System.Collections.Concurrent;
using NSL.Utils;
using NSL.EndPointBuilder;
using NSL.SocketServer.Utils;
using NSL.UDP;
using NSL.BuilderExtensions.UDPServer;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;

namespace NSL.Node.P2Proxy.Client
{
    public partial class P2ProxyEntry
    {
        protected INetworkListener Listener { get; private set; }

        public int ClientBindingPort => Entry.Configuration.GetValue<int>("client_binding_port", 5980);

        public string ClientBindingPoint => Entry.Configuration.GetValue("client_binding_point", default(string));

        protected RoomServerStartupEntry Entry { get; }

        protected ILogger Logger { get; }

        public static P2ProxyEntry Create(
            RoomServerStartupEntry entry,
            string logPrefix = "[P2Proxy]")
            => new P2ProxyEntry(entry, logPrefix);

        public P2ProxyEntry(RoomServerStartupEntry entry, string logPrefix = "[P2Proxy]")
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public P2ProxyEntry Run()
        {
            string bindingPoint = ClientBindingPoint;

            if (bindingPoint == default)
                bindingPoint = $"udp://0.0.0.0:{ClientBindingPort}/";

            var p = NSLEndPoint.FromUrl(bindingPoint);

            Listener = Fill(UDPServerEndPointBuilder.Create()
                .WithClientProcessor<P2PNetworkClient>()
                .WithOptions<UDPClientOptions<P2PNetworkClient>>()
                .WithBindingPoint(p.Address, p.Port))
                .Build();

            Listener.Start();

            return this;
        }

        private TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<P2PNetworkClient>, IHandleIOBuilder
        {
            builder.SetLogger(Logger);

            builder.AddPacketHandle(
                RoomPacketEnum.SignSession, SignInPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.Transport, TransportPacketHandle);
            builder.AddPacketHandle(
                RoomPacketEnum.Broadcast, BroadcastPacketHandle);

            builder.AddDefaultEventHandlers<TBuilder, P2PNetworkClient>(null,
                DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace & ~DefaultEventHandlersEnum.Receive & ~DefaultEventHandlersEnum.Send);

            builder.AddBaseSendHandle((client, pid, len, stack) =>
            {
                if (pid < ushort.MaxValue - 100)
                    Logger.AppendInfo($"Send packet {pid}");
            });

            builder.AddBaseReceiveHandle((client, pid, len) =>
            {
                if (pid < ushort.MaxValue - 100)
                    Logger.AppendInfo($"Receive packet {pid}");
            });

            return builder;
        }

        private ConcurrentDictionary<string, Lazy<ProxyRoomInfo>> roomMap = new();
    }
}
