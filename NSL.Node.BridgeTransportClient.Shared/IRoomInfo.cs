using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeTransportClient.Shared
{
    public interface IRoomInfo
    {
        void Broadcast(OutputPacketBuffer packet);

        void SendTo(Guid nodeId, OutputPacketBuffer packet);

        void SendTo(PlayerInfo player, OutputPacketBuffer packet, bool disposeOnSend = true);
    }
}
