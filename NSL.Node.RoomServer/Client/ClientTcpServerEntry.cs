using NSL.BuilderExtensions.TCPServer;
using NSL.Node.RoomServer.Client.Data;
using NSL.SocketServer;

namespace NSL.Node.RoomServer.Client
{
    public class ClientTcpServerEntry : ClientServerBaseEntry
    {
        private readonly int bindingPort;

        public ClientTcpServerEntry(NodeRoomServerEntry entry, int bindingPort, string logPrefix = null) : base(entry, logPrefix)
        {
            this.bindingPort = bindingPort;
        }

        public override void Run()
        {
            if (Listener != null)
                return;

            Listener = Fill(TCPServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<ServerOptions<TransportNetworkClient>>()
                .WithBindingPoint(bindingPort))
                .Build();

            Listener.Start();
        }
    }
}
