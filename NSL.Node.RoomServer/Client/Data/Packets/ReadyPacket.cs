using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerBaseEntry
    {
        private void ReadyPacketHandle(TransportNetworkClient client, InputPacketBuffer data)
        {
            var result = data
                .CreateWaitBufferResponse()
                .WithPid(RoomPacketEnum.Response);

            if (client.Room == default)
            {
                throw new Exception();
            }

            result.WriteBool(client.Room != default && client.Room.ValidateNodeReady(client, data.ReadInt32(), data.ReadCollection(p => p.ReadGuid())));

            client.Network.Send(result);
        }
    }
}
