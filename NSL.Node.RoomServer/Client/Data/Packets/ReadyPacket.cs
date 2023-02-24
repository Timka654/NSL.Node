using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        private void ReadyPacketHandle(TransportNetworkClient client, InputPacketBuffer data)
        {
            if (!client.are.WaitOne(5000))
                Logger.ConsoleLog(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Error readyPacketHandle");

            var result = data
                .CreateWaitBufferResponse()
                .WithPid(RoomPacketEnum.ReadyNodeResult);

            if (client.Room == default)
            {
                throw new Exception();
            }

            result.WriteBool(client.Room != default && client.Room.ValidateNodeReady(client, data.ReadInt32(), data.ReadCollection(p => p.ReadGuid())));

            client.Network.Send(result);
        }
    }
}
