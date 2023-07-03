using NSL.LocalBridge;
using NSL.SocketCore.Utils;
using NetworkClient = NSL.Node.BridgeServer.LS.LobbyServerNetworkClient;

namespace NSL.Node.BridgeServer.LS
{
    public static class BuilderExtensions
    {
        public static NodeBridgeServerEntryBuilder WithLobbyServerLocalBridgeBinding<TAnotherClient>(
            this NodeBridgeServerEntryBuilder builder,
            out LocalBridgeClient<NetworkClient, TAnotherClient> bridge,
            string logPrefix = null)
            where TAnotherClient : INetworkClient, new()
        {
            var local = new LobbyServerLocalBridgeEntry(builder.Entry, logPrefix);

            bridge = local.CreateLocalBridge<TAnotherClient>();

            return builder.WithLobbyServerListener(local);
        }

        public static NodeBridgeServerEntryBuilder WithLobbyServerBinding(this NodeBridgeServerEntryBuilder builder, int bindingPort,
            string logPrefix = null)
            => builder.WithLobbyServerListener(new LobbyServerEntry(builder.Entry, bindingPort, logPrefix));


        public static NodeBridgeServerEntryBuilder WithLobbyServerBinding(
            this NodeBridgeServerEntryBuilder builder,
            string bindingPoint,
            string logPrefix = null)
            => builder.WithLobbyServerListener(new LobbyServerEntry(builder.Entry, bindingPoint, logPrefix));
    }
}
