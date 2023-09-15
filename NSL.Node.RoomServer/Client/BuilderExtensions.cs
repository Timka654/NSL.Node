using NSL.LocalBridge;
using NSL.Node.RoomServer.Client.Data;
using NSL.SocketCore.Utils;

namespace NSL.Node.RoomServer.Client
{
    public static class BuilderExtensions
    {
        public static NodeRoomServerEntryBuilder WithClientServerLocalBridgeBinding<TAnotherClient>(
            this NodeRoomServerEntryBuilder builder,
            out LocalBridgeClient<TransportNetworkClient, TAnotherClient> bridge,
            string logPrefix = null)
            where TAnotherClient : INetworkClient, new()
        {
            var local = new ClientServerLocalBridgeEntry(builder.Entry, logPrefix);

            bridge = local.CreateLocalBridge<TAnotherClient>();

            return builder.WithClientServerListener(local);
        }

        public static NodeRoomServerEntryBuilder WithClientServerBinding(this NodeRoomServerEntryBuilder builder, int bindingPort,
            string logPrefix = null)
            => builder.WithClientServerListener(new ClientServerEntry(builder.Entry, bindingPort, logPrefix));

        public static NodeRoomServerEntryBuilder WithClientServerBinding(this NodeRoomServerEntryBuilder builder, string bindingPoint,
            string logPrefix = null)
            => builder.WithClientServerListener(new ClientServerEntry(builder.Entry, bindingPoint, logPrefix));
    }
}
