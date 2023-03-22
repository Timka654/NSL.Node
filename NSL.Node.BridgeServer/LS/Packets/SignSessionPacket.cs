using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;

namespace NSL.Node.BridgeServer.LS.Packets
{
    internal class SignSessionPacket
    {
        public static void ReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeLobbyPacketEnum.Response);

            client.Identity = data.ReadString16();

            bool result = client.Entry.LobbyManager.TryLobbyServerConnect(client, data.ReadString16());

            packet.WriteBool(result);

            client.Network.Send(packet);
        }
    }
}
