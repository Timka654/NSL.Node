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
            {
                Entry.Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"Player {client?.Network.GetRemotePoint()} is not (success?) signed");

                await Task.Delay(2_000);

                result.WriteBool(false);
            }
            else
            {
                var request = RoomNodeReadyRequestModel.ReadFullFrom(data);

                result.WriteBool(await client.Room.ValidateNodeReady(client, request.ConnectedNodesCount, request.ConnectedNodes));
            }

            client.Network.Send(result);
        }
    }
}
