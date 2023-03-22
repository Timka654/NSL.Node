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

                        if (!await TrySign())
                        {
                            network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Invalid identity data");
                            client.Network.Disconnect();
                            return;
                        }


                        network.ConnectionOptions.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, "Success connected");

                        OnStateChanged(State);
                    });

                    builder.AddDefaultEventHandlers<WebSocketsClientEndPointBuilder<BridgeRoomNetworkClient, WSClientOptions<BridgeRoomNetworkClient>>, BridgeRoomNetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);
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

        private async Task<bool> TrySign()
        {
            var client = network.Data;

            var output = WaitablePacketBuffer.Create(NodeBridgeRoomPacketEnum.SignServerRequest);

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

            var output = WaitablePacketBuffer.Create(NodeBridgeRoomPacketEnum.SignSessionRequest);

            output.WriteString16(client.Token);

            output.WriteGuid(client.Id);

            await bridgeClient.PacketWaitBuffer.SendWaitRequest(output, data =>
            {
                signResult = data.ReadBool();

                if (signResult)
                {
                    client.LobbyServerIdentity = data.ReadString16();
                    client.RoomId = data.ReadGuid();
                }
                return Task.CompletedTask;
            });

            return signResult;
        }

        internal async Task<(bool, NodeRoomStartupInfo)> GetRoomStartupInfo(RoomInfo room)
        {
            var bridgeClient = network.Data;

            bool signResult = false;

            NodeRoomStartupInfo startupInfo = default;

            var output = WaitablePacketBuffer.Create(NodeBridgeRoomPacketEnum.RoomStartupInfoRequest);

            output.WriteString16(room.LobbyServerIdentity);

            output.WriteGuid(room.RoomId);

            await bridgeClient.PacketWaitBuffer.SendWaitRequest(output, data =>
            {
                signResult = data.ReadBool();

                if (signResult)
                {
                    startupInfo = new NodeRoomStartupInfo(data.ReadCollection(p => new KeyValuePair<string, string>(p.ReadString16(), p.ReadString16())));
                }
                return Task.CompletedTask;
            });

            return (signResult, startupInfo);
        }

        internal void FinishRoom(RoomInfo room, byte[] data)
        {
            var bridgeClient = network.Data;

            var output = WaitablePacketBuffer.Create(NodeBridgeRoomPacketEnum.FinishRoomMessage);

            output.WriteString16(room.LobbyServerIdentity);

            output.WriteGuid(room.RoomId);

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