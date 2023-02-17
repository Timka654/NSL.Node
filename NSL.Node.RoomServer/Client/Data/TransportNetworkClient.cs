using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;
using System;
using NSL.Node.RoomServer.Shared.Client.Core;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;

namespace NSL.Node.RoomServer.Client.Data
{
    public class TransportNetworkClient : IServerNetworkClient, IPlayerNetwork
    {
        public string Token { get; set; }

        public Guid Id { get; set; }

        public string LobbyServerIdentity { get; set; }

        public Guid RoomId { get; set; }

        public string EndPoint { get; set; }

        public bool Ready { get; set; }

        public RoomInfo Room { get; set; }

        public PlayerInfo Player { get; set; }

        public void Send(OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            if (Network != null)
                Network.Send(packet, disposeOnSend);
        }

        public void Transport(Action<OutputPacketBuffer> build, ushort code)
        {
            Transport(p =>
            {
                p.WriteUInt16(code);
                build(p);
            });
        }

        public void Transport(Action<OutputPacketBuffer> build)
        {
            var packet = new OutputPacketBuffer();

            packet.WriteGuid(Id);

            build(packet);

            packet.WithPid(RoomPacketEnum.Transport);

            Send(packet, false);

            packet.Dispose();
        }
    }
}
