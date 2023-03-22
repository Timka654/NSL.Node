using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;
using System;
using NSL.Node.RoomServer.Shared.Client.Core;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.WebSockets.Server.AspNetPoint;
using System.Threading;
using NSL.UDP.Enums;
using NSL.UDP;

namespace NSL.Node.RoomServer.Client.Data
{
    public class TransportNetworkClient : AspNetWSNetworkServerClient, INodeClientNetwork
    {
        public string Token { get; set; }

        public Guid Id { get; set; }

        public string LobbyServerIdentity { get; set; }

        public Guid RoomId { get; set; }

        public string EndPoint { get; set; }

        public bool Ready { get; set; }

        public RoomInfo Room { get; set; }

        public NodeInfo Node { get; set; }

        public Guid NodeId { get; set; }

        public bool IsLocalNode => false;

        public void Send(DgramOutputPacketBuffer packet, UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered, bool disposeOnSend = true)
        {
            if (Network != null)
                Network.Send(packet, disposeOnSend);
            else if (disposeOnSend)
                packet.Dispose();
        }

        public void Send(ushort code, Action<DgramOutputPacketBuffer> build, UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered)
        {
            Send(p =>
            {
                p.WriteUInt16(code);
                build(p);
            });
        }

        public void Send(Action<DgramOutputPacketBuffer> build, UDPChannelEnum channel = UDPChannelEnum.ReliableOrdered)
        {
            var packet = new DgramOutputPacketBuffer();

            packet.WriteGuid(Id);

            build(packet);

            packet.WithPid(RoomPacketEnum.Transport);

            Send(packet, channel, true);
        }

        public void Send(DgramOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (Network != null)
                Network.Send(packet, disposeOnSend);
            else if (disposeOnSend)
                packet.Dispose();
        }

        public void Send(ushort code, Action<DgramOutputPacketBuffer> build)
        {
            Send(p =>
            {
                p.WriteUInt16(code);
                build(p);
            });
        }

        public void Send(Action<DgramOutputPacketBuffer> build)
        {
            var packet = new DgramOutputPacketBuffer();

            packet.WriteGuid(Id);

            build(packet);

            packet.WithPid(RoomPacketEnum.Transport);

            Send(packet, true);
        }

        public void SetObjectOwner(INodeOwneredObject _object)
        {
            _object.SetOwner(Room, this);
        }
    }
}
