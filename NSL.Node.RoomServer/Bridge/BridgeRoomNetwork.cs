using NSL.BuilderExtensions.WebSocketsClient;
using NSL.SocketCore.Extensions.Buffer;
using NSL.WebSockets.Client;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.SocketCore;
using NSL.Node.RoomServer.Client.Data;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeServer.Shared;
using System.Collections.Generic;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;

namespace NSL.Node.RoomServer.Bridge
{
    public delegate Task<bool> ValidateSessionDelegate(string sessionIdentity);

    public class BridgeRoomNetwork
    {
        public Guid ServerIdentity { get; private set; } = Guid.Empty;

        public string IdentityKey => Entry.Configuration.GetValue<string>("bridge_identity_key", "AABBCC");

        public string BridgeAddress => Entry.Configuration.GetValue<string>("bridge_address", "ws://localhost:6998");

        protected WSNetworkClient<BridgeRoomNetworkClient, WSClientOptions<BridgeRoomNetworkClient>> network { get; private set; }

        protected RoomServerStartupEntry Entry { get; }

        protected ILogger Logger { get; }

        public static BridgeRoomNetwork Create(RoomServerStartupEntry entry, string logPrefix = "[BridgeClient]")
            => new BridgeRoomNetwork(entry, logPrefix);

        public BridgeRoomNetwork(RoomServerStartupEntry entry, string logPrefix = "[BridgeClient]")
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public BridgeRoomNetwork Run()
        {
            network = WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeRoomNetworkClient>()
                .WithOptions<WSClientOptions<BridgeRoomNetworkClient>>()
                .WithUrl(new Uri(BridgeAddress))
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddReceivePacketHandle(
                        NodeBridgeRoomPacketEnum.Response,
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
                        IdentityFailed = false;

                        client.PingPongEnabled = true;

                        if (!(await TrySign()).Result)
                        {
                            network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Invalid identity data");
                            client.Network.Disconnect();
                            return;
                        }


                        network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, "Success connected");

                        OnStateChanged(State);
                    });

                    builder.AddDefaultEventHandlers<WebSocketsClientEndPointBuilder<BridgeRoomNetworkClient, WSClientOptions<BridgeRoomNetworkClient>>, BridgeRoomNetworkClient>(null,
                DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace & ~DefaultEventHandlersEnum.Receive & ~DefaultEventHandlersEnum.Send);

                    builder.AddBaseSendHandle((client, pid, len, stack) =>
                    {
                        if (pid < ushort.MaxValue - 100)
                            Logger.AppendInfo($"Send packet {pid}");
                    });

                    builder.AddBaseReceiveHandle((client, pid, len) =>
                    {
                        if (pid < ushort.MaxValue - 100)
                            Logger.AppendInfo($"Receive packet {pid}");
                    });
                })
                .Build();

            return this;
        }

        bool _signResult = false;

        bool signResult { get => _signResult; set { _signResult = value; OnStateChanged(State); } }


        bool authorized = false;

        string transportEndPoint;

        public async void Authorize(string transportEndPoint)
        {
            if (authorized)
                return;

            authorized = true;

            this.transportEndPoint = transportEndPoint;

            await TryConnect();
        }

        private async Task<bool> TryConnect(int timeout = 9000)
        {
            Logger?.AppendInfo($"Try connect to Bridge({BridgeAddress})");

            if (!await network.ConnectAsync(timeout))
            {
                network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Cannot connect");

                return false;
            }

            return true;
        }

        private async Task<RoomSignInResponseModel> TrySign()
        {
            var client = network.Data;

            var output = RequestPacketBuffer.Create(NodeBridgeRoomPacketEnum.SignServerRequest);

            new RoomSignInRequestModel()
            {
                Identity = ServerIdentity,
                ConnectionEndPoint = transportEndPoint,
                IdentityKey = IdentityKey
            }.WriteFullTo(output);

            _signResult = false;

            RoomSignInResponseModel response = default;

            await client.PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                response = RoomSignInResponseModel.ReadFullFrom(data);

                return Task.CompletedTask;
            });

            response ??= new RoomSignInResponseModel();

            _signResult = response.Result;

            IdentityFailed = !_signResult;

            if (_signResult)
                ServerIdentity = response.ServerIdentity;

            OnStateChanged(State);

            return response;
        }

        internal async Task<RoomSignSessionResponseModel> TrySignSession(RoomSignSessionRequestModel data)
        {
            var bridgeClient = network.Data;

            var output = RequestPacketBuffer.Create(NodeBridgeRoomPacketEnum.SignSessionRequest);

            data.WriteFullTo(output);

            RoomSignSessionResponseModel result = default;

            await bridgeClient.PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                result = RoomSignSessionResponseModel.ReadFullFrom(data);

                return Task.CompletedTask;
            });

            return result ?? new RoomSignSessionResponseModel() { Result = false };
        }

        internal async Task<RoomSignSessionPlayerResponseModel> TrySignSessionPlayer(RoomSignSessionPlayerRequestModel data)
        {
            var bridgeClient = network.Data;

            var output = RequestPacketBuffer.Create(NodeBridgeRoomPacketEnum.SignSessionPlayerRequest);

            data.WriteFullTo(output);

            RoomSignSessionPlayerResponseModel result = default;

            await bridgeClient.PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                result = RoomSignSessionPlayerResponseModel.ReadFullFrom(data);

                return Task.CompletedTask;
            });

            return result ?? new RoomSignSessionPlayerResponseModel();
        }

        internal void FinishRoom(RoomInfo room, byte[] data)
        {
            var bridgeClient = network.Data;

            var output = OutputPacketBuffer.Create(NodeBridgeRoomPacketEnum.FinishRoomMessage);

            output.WriteGuid(room.SessionId);

            if (data != null)
                output.Write(data);

            bridgeClient.Send(output);

        }

        internal void RoomMessage(RoomInfo room, byte[] data)
        {
            var bridgeClient = network.Data;

            var output = OutputPacketBuffer.Create(NodeBridgeRoomPacketEnum.RoomMessage);

            output.WriteGuid(room.SessionId);

            if (data != null)
                output.Write(data);

            bridgeClient.Send(output);

        }

        public event Action<bool> OnStateChanged = (state) => { };

        public bool State => network?.GetState() == true && signResult;

        public bool IdentityFailed { get; private set; }

        public ValidateSessionDelegate ValidateSession { set; private get; }
    }
}