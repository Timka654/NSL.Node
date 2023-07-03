using NSL.BuilderExtensions.WebSocketsServer;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerEntry : RoomServerBaseEntry
    {
        private readonly string bindingPoint;

        public RoomServerEntry(NodeBridgeServerEntry entry, string bindingPoint, string logPrefix = null) : base(entry, logPrefix)
        {
            this.bindingPoint = bindingPoint;
        }
        public RoomServerEntry(NodeBridgeServerEntry entry, int bindingPort, string logPrefix = null) : this(entry, $"http://*:{bindingPort}/", logPrefix) { }

        public override void Run()
        {
            Listener = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithBindingPoint(bindingPoint))
                .Build();

            Listener.Start();
        }
    }
}
