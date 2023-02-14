using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        private async void SignInPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var response = OutputPacketBuffer.Create(RoomPacketEnum.SignSessionResult);

            client.Token = buffer.ReadString16();

            client.Id = buffer.ReadGuid();

            client.EndPoint = buffer.ReadString16();

            var result = await bridgeNetwork.TryAuthorize(client);

            response.WriteBool(result);

            if (result)
            {
                client.Room = roomMap.GetOrAdd(client.RoomId, id => {
                    var room = new RoomInfo(id);

                    return room;
                });

                client.Room.AddClient(client);
            }
            else
            {
                client.Token = default;
                client.Id = default;
            }
            client.Network.Send(response);
        }
    }
}
