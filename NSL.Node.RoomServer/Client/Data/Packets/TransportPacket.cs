using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        private void TransportPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            if (!client.are.WaitOne(5000))
                Logger.ConsoleLog(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Error transportPacketHandle");

            client.Room?.Transport(client, buffer);
        }
    }
}
