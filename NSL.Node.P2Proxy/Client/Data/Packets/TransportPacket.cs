using NSL.Node.P2Proxy.Client.Data;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.P2Proxy.Client
{
    public partial class P2ProxyEntry
    {
        private void TransportPacketHandle(P2PNetworkClient client, InputPacketBuffer buffer)
        {
            client.Room?.Transport(client, buffer);
        }
    }
}
