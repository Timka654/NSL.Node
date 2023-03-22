using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Node.BridgeServer.Shared;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using System;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient
{
    public delegate Task<bool> ValidateSessionDelegate(Guid roomId, string sessionIdentity);
    public delegate Task<bool> RoomStartupInfoDelegate(Guid roomId, NodeRoomStartupInfo startupInfo);
    public delegate Task RoomFinishDelegate(Guid roomId, byte[] data);

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
                         BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.Response,
                         c => c.PacketWaitBuffer);

                     builder.AddPacketHandle(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.ValidateSessionRequest, async (c, d) =>
                     {
                         var packet = d.CreateWaitBufferResponse().WithPid(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.Response);

                         var roomId = d.ReadGuid();
                         var sessionId = d.ReadString16();

                         bool result = default;

                         if (ValidateSession != null)
                             result = await ValidateSession(roomId, sessionId);

                         packet.WriteBool(result);

                         c.Network.Send(packet);
                     });

                     builder.AddPacketHandle(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.FinishRoomMessage, async (c, d) =>
                     {
                         var roomId = d.ReadGuid();

                         var dataLen = d.DataLength - d.DataPosition;

                         byte[] buffer = default;

                         if (dataLen > 0)
                             buffer = d.Read(dataLen);
                         else
                             buffer = new byte[0];

                         await RoomFinish(roomId, buffer);
                     });

                     builder.AddPacketHandle(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.RoomStartupInfoRequest, async (c, d) =>
                     {
                         var packet = d.CreateWaitBufferResponse().WithPid(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.Response);

                         var roomId = d.ReadGuid();
                         bool result = default;

                         NodeRoomStartupInfo startupInfo = new NodeRoomStartupInfo();

                         if (RoomStartupInfo != null)
                             result = await RoomStartupInfo(roomId, startupInfo);

                         packet.WriteBool(result);

                         if (result)
                             packet.WriteCollection(startupInfo.GetCollection(), item =>
                             {
                                 packet.WriteString16(item.Key);
                                 packet.WriteString16(item.Value);
                             });

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

            var output = WaitablePacketBuffer.Create(BridgeServer.Shared.Enums.NodeBridgeLobbyPacketEnum.SignServerRequest);

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

        public RoomStartupInfoDelegate RoomStartupInfo { set; private get; }

        public RoomFinishDelegate RoomFinish { set; private get; }
    }
}
