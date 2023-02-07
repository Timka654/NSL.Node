using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeTransportClient.Transport.Data;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Transport
{
    public partial class TransportNetwork<TRoomInfo>
        where TRoomInfo : RoomInfo, new()
    {
        private void ExecutePacketHandle(TransportNetworkClient<TRoomInfo> client, InputPacketBuffer buffer)
        {
            client.Room?.Execute(client, buffer);
        }
    }
}
