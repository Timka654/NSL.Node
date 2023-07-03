using NSL.BuilderExtensions.LocalBridge;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.LocalBridge;
using NSL.Node.BridgeLobbyClient.Models;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Client;
using System;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient
{
    public class BridgeLobbyLocalBridgeNetwork<TServerClient> : BridgeLobbyBaseNetwork
        where TServerClient : INetworkClient, new()
    {
        LocalBridgeClient<TServerClient, BridgeLobbyNetworkClient> serverNetwork;
        LocalBridgeClient<BridgeLobbyNetworkClient, TServerClient> localNetwork;

        public BridgeLobbyLocalBridgeNetwork(string serverIdentity, string identityKey, Action<BridgeLobbyNetworkHandlesConfigurationModel> onHandleConfiguration, Action<WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null) : base(serverIdentity, identityKey, onHandleConfiguration)
        {
            var builder = FillOptions(WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeLobbyNetworkClient>()
                .WithOptions<WSClientOptions<BridgeLobbyNetworkClient>>(), onBuild);

            localNetwork =  builder
                .CreateLocalBridge<BridgeLobbyNetworkClient, TServerClient>();

            var bridgeNetwork = localNetwork.GetUserData() as BridgeLobbyNetworkClient;
        }

        public BridgeLobbyLocalBridgeNetwork<TServerClient> WithServerClient(LocalBridgeClient<TServerClient, BridgeLobbyNetworkClient> serverClient)
        {
            serverNetwork = serverClient;

            return this;
        }

        protected override Task<bool> InitNetwork()
        {
            localNetwork.SetOtherClient(serverNetwork);

            return Task.FromResult(true);
        }
    }
}
