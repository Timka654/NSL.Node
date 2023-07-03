using NSL.BuilderExtensions.WebSocketsServer;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NSL.BuilderExtensions.LocalBridge;
using NSL.LocalBridge;
using NSL.SocketCore.Utils;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerLocalBridgeEntry : RoomServerBaseEntry
    {
        public RoomServerLocalBridgeEntry(NodeBridgeServerEntry entry, string logPrefix = null) : base(entry, logPrefix) { }

        public LocalBridgeClient<NetworkClient, TAnotherClient> CreateLocalBridge<TAnotherClient>()
            where TAnotherClient : INetworkClient, new()
        {
            return Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()).CreateLocalBridge<NetworkClient, TAnotherClient>();
        }

        public override void Run()
        {
        }
    }
}
