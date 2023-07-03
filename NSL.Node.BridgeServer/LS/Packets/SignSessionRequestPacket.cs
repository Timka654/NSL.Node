using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Utils;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;

namespace NSL.Node.BridgeServer.LS.Packets
{
    internal class SignSessionRequestPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateResponse();

            var request = LobbySignInRequestModel.ReadFullFrom(data);

            client.Identity = request.Identity;

            bool result = client.Entry.LobbyManager.TryLobbyServerConnect(client, request);

            packet.WriteBool(result);

            client.Network.Send(packet);
        }
    }
}
