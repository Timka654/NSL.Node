using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeTransportClient.Shared;
using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Transport.Data
{
    public class TransportNetworkClient : IServerNetworkClient, IPlayerNetwork
    {
        public string Token { get; set; }

        public Guid Id { get; set; }

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

            packet.WithPid(NodeTransportPacketEnum.Transport);

            Send(packet, false);

            packet.Dispose();
        }
    }
}
