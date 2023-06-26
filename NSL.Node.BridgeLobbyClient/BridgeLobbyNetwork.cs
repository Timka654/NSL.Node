using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Node.BridgeLobbyClient.Models;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.SocketCore.Extensions.Buffer;
using NSL.WebSockets.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient
{
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
            Action<BridgeLobbyNetworkHandlesConfigurationModel> onHandleConfiguration,
            Action<WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null)
        {
            WsUrl = wsUrl;
            ServerIdentity = serverIdentity;
            IdentityKey = identityKey;

            OnStateChanged += state =>
            {
                if (HasSuccessConnections)
                    return;

                if (state)
                    HasSuccessConnections = state;
            };

            if (onHandleConfiguration != null)
                onHandleConfiguration(HandleConfiguration);

            network = WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeLobbyNetworkClient>()
                .WithOptions<WSClientOptions<BridgeLobbyNetworkClient>>()
                .WithUrl(wsUrl)
                .WithCode(builder =>
                 {
                     builder.AddReceivePacketHandle(
                         NodeBridgeLobbyPacketEnum.Response,
                         c => c.PacketWaitBuffer);

                     builder.AddPacketHandle(
                         NodeBridgeLobbyPacketEnum.FinishRoomMessage,
                         Packets.FinishRoomMessagePacket.Handle);

                     builder.AddPacketHandle(
                         NodeBridgeLobbyPacketEnum.RoomMessage,
                         Packets.RoomMessagePacket.Handle);

                     builder.AddDisconnectHandle(DisconnectHandle);

                     builder.AddConnectHandle(ConnectHandle);

                     if (onBuild != null)
                         onBuild(builder);
                 })
                .Build();
        }

        private async void DisconnectHandle(BridgeLobbyNetworkClient client)
        {
            signResult = false;

            OnStateChanged(State);

            await Task.Delay(4_000);

            await TryConnect();
        }

        private async void ConnectHandle(BridgeLobbyNetworkClient client)
        {
            client.HandlesConfiguration = HandleConfiguration;

            client.PingPongEnabled = true;

            if (!await TrySign())
            {
                network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Invalid identity data");
                client.Network.Disconnect();
                return;
            }

            network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, "Success connected");

            OnStateChanged(State);
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

            var output = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.SignServerRequest);

            new LobbySignInRequestModel()
            {
                Identity = ServerIdentity,
                IdentityKey = IdentityKey,
                IsRecovery = HasSuccessConnections
            }.WriteFullTo(output);

            bool signResult = false;

            await client.PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                signResult = data.ReadBool();

                IdentityFailed = !signResult;

                OnStateChanged(State);

                return Task.CompletedTask;
            });

            return signResult;
        }

        public async Task<CreateRoomSessionResponseModel> CreateRoom(LobbyCreateRoomSessionRequestModel roomInfo)
        {
            var client = network.Data;

            var output = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.CreateRoomSessionRequest);

            roomInfo.WriteFullTo(output);

            CreateRoomSessionResponseModel response = default;

            await client.PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                response = CreateRoomSessionResponseModel.ReadFullFrom(data);

                return Task.CompletedTask;
            });

            return response;
        }

        public async Task AddPlayer(LobbyRoomPlayerAddRequestModel playerInfo)
        {
            var client = network.Data;

            var output = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.AddPlayerRequest);

            playerInfo.WriteFullTo(output);

            await client.PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                return Task.CompletedTask;
            });
        }

        public async Task RemovePlayer(LobbyRoomPlayerRemoveRequestModel playerInfo)
        {
            var client = network.Data;

            var output = RequestPacketBuffer.Create(NodeBridgeLobbyPacketEnum.RemovePlayerRequest);

            playerInfo.WriteFullTo(output);

            await client.PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                return Task.CompletedTask;
            });
        }

        public event Action<bool> OnStateChanged = (state) => { };

        public bool State => network?.GetState() == true && signResult;

        public bool IdentityFailed { get; private set; }

        private bool HasSuccessConnections { get; set; }

        internal BridgeLobbyNetworkHandlesConfigurationModel HandleConfiguration { get; set; } = new BridgeLobbyNetworkHandlesConfigurationModel();
    }
}
