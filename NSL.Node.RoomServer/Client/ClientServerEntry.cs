using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.RoomServer.Client.Data;
using NSL.WebSockets.Server;

namespace NSL.Node.RoomServer.Client
{
    public class ClientServerEntry : ClientServerBaseEntry
    {
        private readonly string bindingPoint;

        public ClientServerEntry(NodeRoomServerEntry entry, string bindingPoint, string logPrefix = null) : base(entry, logPrefix)
        {
            this.bindingPoint = bindingPoint;
        }
        public ClientServerEntry(NodeRoomServerEntry entry, int bindingPort, string logPrefix = null) : this(entry, $"http://*:{bindingPort}/", logPrefix) { }

        public override void Run()
        {
            if (Listener != null)
                return;

            Listener = Fill(WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>()
                .WithBindingPoint(bindingPoint))
                .Build();

            Listener.Start();
        }
    }
}
