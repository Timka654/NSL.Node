using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Node.BridgeLobbyClient.Models;
using NSL.WebSockets.Client;
using System;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient
{
    public class BridgeLobbyNetwork : BridgeLobbyBaseNetwork
    {
        private WSNetworkClient<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>> wsNetwork;

        public BridgeLobbyNetwork(Uri wsUrl, string serverIdentity, string identityKey, Action<BridgeLobbyNetworkHandlesConfigurationModel> onHandleConfiguration, Action<WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null) : base(serverIdentity, identityKey, onHandleConfiguration)
        {
            wsNetwork = FillOptions(WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeLobbyNetworkClient>()
                .WithOptions<WSClientOptions<BridgeLobbyNetworkClient>>()
                .WithUrl(wsUrl), onBuild).Build();
        }

        protected override async Task<bool> InitNetwork()
        {
            if (!await wsNetwork.ConnectAsync(3000))
            {
                network.Network.Options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Cannot connect");

                return false;
            }

            return true;
        }
    }
}
