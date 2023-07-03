using NSL.BuilderExtensions.LocalBridge;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.LocalBridge;
using NSL.SocketCore.Utils;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerLocalBridgeEntry : LobbyServerBaseEntry
    {
        public LobbyServerLocalBridgeEntry(NodeBridgeServerEntry entry, string logPrefix = null) : base(entry, logPrefix)
        {
        }

        public LocalBridgeClient<NetworkClient, TAnotherClient> CreateLocalBridge<TAnotherClient>()
            where TAnotherClient: INetworkClient, new()
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
