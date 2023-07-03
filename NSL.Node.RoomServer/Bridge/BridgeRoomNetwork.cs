using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Logger;
using NSL.WebSockets.Client;
using System;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Bridge
{
    public class BridgeRoomNetwork : BridgeRoomBaseNetwork
    {
        private readonly string bindingPoint;
        private WSNetworkClient<BridgeRoomNetworkClient, WSClientOptions<BridgeRoomNetworkClient>> wsNetwork;

        public BridgeRoomNetwork(NodeRoomServerEntry entry, Uri wsUrl, string publicEndPoint, string identityKey, Guid serverId = default, string logPrefix = null) : base(entry, identityKey, publicEndPoint, serverId, logPrefix)
        {
            bindingPoint = wsUrl.ToString();

            wsNetwork = FillOptions(WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeRoomNetworkClient>()
                .WithOptions<WSClientOptions<BridgeRoomNetworkClient>>()
                .WithUrl(wsUrl))
                .Build();
        }

        protected override async Task<bool> InitNetwork()
        {
            Logger?.AppendInfo($"Try connect to Bridge({bindingPoint})");

            if (!await wsNetwork.ConnectAsync(3000))
            {
                Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Cannot connect");

                return false;
            }

            return true;
        }
    }
}
