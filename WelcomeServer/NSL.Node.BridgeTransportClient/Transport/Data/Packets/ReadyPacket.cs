using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeTransportClient.Transport.Data;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NSL.Node.BridgeTransportClient.Transport
{
    public partial class TransportNetwork
    {
        private static void ReadyPacketHandle(TransportNetworkClient client, InputPacketBuffer data)
        {
            var result = data
                .CreateWaitBufferResponse()
                .WithPid(NodeTransportPacketEnum.ReadyNodeResultPID);

            result.WriteBool(client.Room != default && client.Room.ValidateNodeReady(client, data.ReadInt32(), data.ReadCollection(p => p.ReadGuid())));

            client.Network.Send(result);
        }
    }
}
