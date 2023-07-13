using NSL.Node.Core.Models.Requests;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerBaseEntry
    {
        private async Task ReadyPacketHandle(TransportNetworkClient client, InputPacketBuffer data)
        {
            var result = data.CreateResponse();

            if (client.Room == default)
                throw new Exception($"Player {client?.Network.GetRemotePoint()} is not (success?) signed");

            var request = RoomNodeReadyRequestModel.ReadFullFrom(data);

            result.WriteBool(await client.Room.ValidateNodeReady(client, request.ConnectedNodesCount, request.ConnectedNodes));

            client.Network.Send(result);
        }
    }
}
