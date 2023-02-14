using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;


namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        private static void ReadyPacketHandle(TransportNetworkClient client, InputPacketBuffer data)
        {
            var result = data
                .CreateWaitBufferResponse()
                .WithPid(RoomPacketEnum.ReadyNodeResult);

            result.WriteBool(client.Room != default && client.Room.ValidateNodeReady(client, data.ReadInt32(), data.ReadCollection(p => p.ReadGuid())));

            client.Network.Send(result);
        }
    }
}
