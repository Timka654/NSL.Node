using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        private void TransportPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            client.Room?.Transport(client, buffer);
        }
    }
}
