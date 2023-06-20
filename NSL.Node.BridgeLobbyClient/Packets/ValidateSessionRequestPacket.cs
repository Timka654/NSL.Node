using NSL.Node.BridgeLobbyClient.Utils;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeLobbyClient.Packets
{
    internal class ValidateSessionRequestPacket
    {
        public static async void Handle(BridgeLobbyNetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var roomId = data.ReadGuid();
            var sessionId = data.ReadString16();

            bool result = default;

            if (client.HandlesConfiguration.ValidateSessionHandle != null)
                result = await client.HandlesConfiguration.ValidateSessionHandle(roomId, sessionId);

            packet.WriteBool(result);

            client.Network.Send(packet);
        }

    }
}
