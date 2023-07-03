using NSL.BuilderExtensions.LocalBridge;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.LocalBridge;
using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Server;

namespace NSL.Node.RoomServer.Client
{
    public class ClientServerLocalBridgeEntry : ClientServerBaseEntry
    {
        public ClientServerLocalBridgeEntry(NodeRoomServerEntry entry, string logPrefix = null) : base(entry, logPrefix) { }

        public LocalBridgeClient<TransportNetworkClient, TAnotherClient> CreateLocalBridge<TAnotherClient>()
            where TAnotherClient : INetworkClient, new()
        {
            return Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>()).CreateLocalBridge<TransportNetworkClient, TAnotherClient>();
        }

        public override void Run()
        {
        }
    }
}
