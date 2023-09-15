using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils.Buffer;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerBaseEntry
    {
        private async void DisconnectMessagePacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            client.DisconnectedFromNodeSide = true;

            await Task.Delay(1000);

            client.Network.Disconnect();
        }
    }
}
