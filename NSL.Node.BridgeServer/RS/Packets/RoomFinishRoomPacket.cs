using NSL.Node.BridgeServer.Shared.Message;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;

namespace NSL.Node.BridgeServer.RS.Packets
{
    internal class RoomFinishRoomPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var message = RoomFinishMessageModel.ReadFullFrom(data);

            client.GetSession(message.SessionId)?.SendLobbyFinishRoom(message.Data, true);
        }
    }
}
