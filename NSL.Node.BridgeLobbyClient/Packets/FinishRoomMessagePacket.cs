using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeLobbyClient.Packets
{
    internal class FinishRoomMessagePacket
    {
        public static async void Handle(BridgeLobbyNetworkClient client, InputPacketBuffer data)
        {
            var roomId = data.ReadGuid();

            var dataLen = data.DataLength - data.DataPosition;

            byte[] buffer = default;

            if (dataLen > 0)
                buffer = data.Read(dataLen);

            if (client.HandlesConfiguration.RoomFinishHandle != null)
                await client.HandlesConfiguration.RoomFinishHandle(roomId, buffer);
        }
    }
}
