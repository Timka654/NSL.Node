using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeTransportClient.Bridge;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using NSL.WebSockets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Transport
{
    public class TransportNetwork
    {
        public string BindingAddress { get; }

        protected WSServerListener<TransportNetworkClient> network { get; private set; }

        public TransportNetwork(
            BridgeTransportNetwork bridgeNetwork,
            string bindingAddress,
            Action<WebSocketsServerEndPointBuilder<TransportNetworkClient, WSServerOptions<TransportNetworkClient>>> onBuild = null)
        {
            this.bridgeNetwork = bridgeNetwork;
            this.BindingAddress = bindingAddress;

            network = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>()
                .WithBindingPoint(bindingAddress)
                .WithCode(builder =>
                {
                    builder.AddPacketHandle(
                        BridgeServer.Shared.Enums.NodeTransportPacketEnum.SignSession,
                        SignInPacketHandle);
                    builder.AddPacketHandle(
                        BridgeServer.Shared.Enums.NodeTransportPacketEnum.Transport, TransportHandle);

                    builder.AddDisconnectHandle(async client =>
                    {
                        await Task.CompletedTask;
                    });

                    builder.AddConnectHandle(async client =>
                    {
                        await Task.CompletedTask;
                    });

                    if (onBuild != null)
                        onBuild(builder);
                })
                .Build();

            network.Start();
        }

        private async void SignInPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var response = buffer.CreateWaitBufferResponse()
                 .WithPid(NodeTransportPacketEnum.SignSessionResult);

            var identityKey = buffer.ReadString16();

            client.Id = buffer.ReadGuid();

            var result = await bridgeNetwork.TryAuthorize(identityKey, client.Id);

            if (!result)
                client.Id = default;

            response.WriteBool(result);

            client.Network.Send(response);
        }

        private void TransportHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var body = buffer.GetBuffer();

            Memory<byte> mem = new Memory<byte>();

            body[..7].CopyTo(mem);
            client.Id.ToByteArray().CopyTo(mem);
            body[7..buffer.Lenght].CopyTo(mem);

            client.Network.Send(mem.ToArray());
        }


        private readonly BridgeTransportNetwork bridgeNetwork;
    }
}
