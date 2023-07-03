using NSL.BuilderExtensions.WebSocketsServer;

using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.LS.LobbyServerNetworkClient>;

namespace NSL.Node.BridgeServer.LS
{
    public class LobbyServerEntry : LobbyServerBaseEntry
    {
        private readonly string bindingPoint;

        public LobbyServerEntry(NodeBridgeServerEntry entry, string bindingPoint, string logPrefix = null) : base(entry, logPrefix)
        {
            this.bindingPoint = bindingPoint;
        }

        public LobbyServerEntry(NodeBridgeServerEntry entry, int bindingPort, string logPrefix = null) : this(entry, $"http://*:{bindingPort}/", logPrefix) { }

        public override void Run()
        {
            Listener = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>())
                .WithBindingPoint(bindingPoint)
                .Build();

            Listener.Start();
        }
    }
}
