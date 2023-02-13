using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.RoomServer.Transport.Data;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Transport
{
    public partial class ClientServerEntry
    {
        private void ExecutePacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            client.Room?.Execute(client, buffer);
        }
    }
}
