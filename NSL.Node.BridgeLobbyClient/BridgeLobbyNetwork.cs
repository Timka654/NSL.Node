using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient
{
    public delegate Task<bool> ValidateSessionDelegate(string sessionIdentity);

    public class BridgeLobbyNetwork
    {
        public Uri WsUrl { get; }

        public string ServerIdentity { get; }

        public string IdentityKey { get; }

        protected WSNetworkClient<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>> network { get; private set; }

        public BridgeLobbyNetwork(
            Uri wsUrl,
            string serverIdentity,
            string identityKey,
            Action<WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null)
        {
            WsUrl = wsUrl;
            ServerIdentity = serverIdentity;
            IdentityKey = identityKey;
            network = WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeLobbyNetworkClient>()
                .WithOptions<WSClientOptions<BridgeLobbyNetworkClient>>()
                .WithUrl(wsUrl)
                .WithCode(builder =>
                 {
                     builder.AddReceivePacketHandle(
                         BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.SignServerResultPID,
                         c => c.PacketWaitBuffer);

                     builder.AddPacketHandle(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.ValidateSessionPID, async (c, d) =>
                     {
                         var packet = d.CreateWaitBufferResponse();

                         packet.WithPid(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.ValidateSessionResultPID);

                         var sessionId = d.ReadString16();

                         bool result = default;

                         if (ValidateSession != null)
                             result = await ValidateSession(sessionId);

                         packet.WriteBool(result);

                         c.Network.Send(packet);
                     });

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

        public async void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

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

            var output = WaitablePacketBuffer.Create(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.SignServerPID);

            output.WriteString16(ServerIdentity);

            output.WriteString16(IdentityKey);

            bool signResult = false;

            await client.PacketWaitBuffer.SendWaitRequest(output, data =>
            {
                signResult = data.ReadBool();

                IdentityFailed = !signResult;

                OnStateChanged(State);

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
