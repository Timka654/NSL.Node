using NSL.Node.BridgeServer.Shared.Message;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class RoomMessagePacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var message = RoomMessageModel.ReadFullFrom(data);

            client.GetSession(message.SessionId).SendLobbyRoomMessage(message.Data);
        }
    }
}
