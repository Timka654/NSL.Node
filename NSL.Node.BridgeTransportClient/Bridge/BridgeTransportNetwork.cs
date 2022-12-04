using NSL.BuilderExtensions.WebSocketsClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.WebSockets.Client;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.SocketCore;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore;
using System.Net;
using NSL.Node.BridgeTransportClient.Transport;

namespace NSL.Node.BridgeTransportClient.Bridge
{
    public delegate Task<bool> ValidateSessionDelegate(string sessionIdentity);

    public class BridgeTransportNetwork
    {
        public Uri WsUrl { get; }

        public Guid ServerIdentity { get; private set; } = Guid.Empty;

        public string IdentityKey { get; }

        protected WSNetworkClient<BridgeTransportNetworkClient, WSClientOptions<BridgeTransportNetworkClient>> network { get; private set; }

        public BridgeTransportNetwork(
            Uri wsUrl,
            string identityKey,
            Action<WebSocketsClientEndPointBuilder<BridgeTransportNetworkClient, WSClientOptions<BridgeTransportNetworkClient>>> onBuild = null)
        {
            WsUrl = wsUrl;
            IdentityKey = identityKey;
            network = WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeTransportNetworkClient>()
                .WithOptions<WSClientOptions<BridgeTransportNetworkClient>>()
                .WithUrl(wsUrl)
                .WithCode(builder =>
                {
                    builder.AddReceivePacketHandle(
                        BridgeServer.Shared.Enums.NodeBridgeTransportPacketEnum.SignServerResultPID,
                        c => c.PacketWaitBuffer);
                    builder.AddReceivePacketHandle(
                        BridgeServer.Shared.Enums.NodeBridgeTransportPacketEnum.SignSessionResultPID,
                        c => c.PacketWaitBuffer);

                    builder.AddDisconnectHandle(async client =>
                    {
                        signResult = false;

                        OnStateChanged(State);

                        await Task.Delay(4_000);

                        await TryConnect();
                    });

                    builder.AddConnectHandle(async client =>
                    {
                        client.PingPongEnabled = true;

                        if (!await TrySign())
                        {
                            network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Invalid identity data");
                            client.Network.Disconnect();
                            return;
                        }


                        network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, "Success connected");

                        OnStateChanged(State);
                    });

                    if (onBuild != null)
                        onBuild(builder);
                })
                .Build();
        }

        bool _signResult = false;

        bool signResult { get => _signResult; set { _signResult = value; OnStateChanged(State); } }


        bool initialized = false;

        string transportEndPoint;

        public async void Initialize(string transportEndPoint)
        {
            if (initialized)
                return;

            initialized = true;

            this.transportEndPoint = transportEndPoint;

            await TryConnect();
        }

        private async Task<bool> TryConnect(int timeout = 3000)
        {
            if (!await network.ConnectAsync(timeout))
            {
                network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Cannot connect");

                return false;
            }

            return true;
        }

        private async Task<bool> TrySign()
        {
            var client = network.Data;

            var output = WaitablePacketBuffer.Create(BridgeServer.Shared.Enums.NodeBridgeTransportPacketEnum.SignServerPID);

            output.WriteGuid(ServerIdentity);

            output.WriteString16(transportEndPoint);

            output.WriteString16(IdentityKey);

            _signResult = false;

            await client.PacketWaitBuffer.SendWaitRequest(output, data =>
            {
                _signResult = data.ReadBool();

                IdentityFailed = !_signResult;

                if (_signResult)
                    ServerIdentity = data.ReadGuid();

                OnStateChanged(State);

                return Task.CompletedTask;
            });

            return signResult;
        }

        internal async Task<bool> TryAuthorize(TransportNetworkClient client)
        {
            var bridgeClient = network.Data;

            bool signResult = false;

            var output = WaitablePacketBuffer.Create(BridgeServer.Shared.Enums.NodeBridgeTransportPacketEnum.SignSessionPID);

            output.WriteString16(client.Token);

            output.WriteGuid(client.Id);

            await bridgeClient.PacketWaitBuffer.SendWaitRequest(output, data =>
            {
                signResult = data.ReadBool();

                if (signResult)
                    client.RoomId = data.ReadGuid();

                return Task.CompletedTask;
            });

            return signResult;
        }

        public event Action<bool> OnStateChanged = (state) => { };

        public bool State => network?.GetState() == true && signResult;

        public bool IdentityFailed { get; private set; }

        public ValidateSessionDelegate ValidateSession { set; private get; }
    }
}