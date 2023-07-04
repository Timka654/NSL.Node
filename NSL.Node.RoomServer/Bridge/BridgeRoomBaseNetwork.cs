﻿using NSL.SocketCore.Extensions.Buffer;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.SocketCore;
using NSL.Node.RoomServer.Client.Data;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.EndPointBuilder;
using NSL.SocketCore.Utils;

namespace NSL.Node.RoomServer.Bridge
{
    public abstract class BridgeRoomBaseNetwork
    {
        public Guid ServerIdentity { get; private set; } = Guid.Empty;

        private readonly string identityKey;
        private readonly string publicEndPoint;

        protected INetworkClient network { get; private set; }

        protected RequestProcessor PacketWaitBuffer { get; private set; }

        protected ILogger Logger { get; }

        public BridgeRoomBaseNetwork(NodeRoomServerEntry entry, string identityKey, string publicEndPoint, Guid serverId = default, string logPrefix = null)
        {
            this.identityKey = identityKey;
            this.publicEndPoint = publicEndPoint;
            ServerIdentity = serverId;

            if (entry.Logger != null)
                Logger = new PrefixableLoggerProxy(entry.Logger, logPrefix ?? "[BridgeClient]");
        }

        protected TBuilder FillOptions<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<BridgeRoomNetworkClient>, IHandleIOBuilder
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

                await InitNetwork();
            });

            builder.AddConnectHandle(async client =>
            {
                IdentityFailed = false;

                client.PingPongEnabled = true;

                network = client;

                PacketWaitBuffer = client.PacketWaitBuffer;

                if (!(await TrySign()).Result)
                {
                    Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, "Invalid identity data");
                    client.Network.Disconnect();
                    return;
                }


                Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, "Success connected");

                OnStateChanged(State);
            });

            builder.AddDefaultEventHandlers<TBuilder,  BridgeRoomNetworkClient>(null,
        DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace & ~DefaultEventHandlersEnum.Receive & ~DefaultEventHandlersEnum.Send);

            builder.AddBaseSendHandle((client, pid, len, stack) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger.AppendInfo($"Send packet {pid}");
            });

            builder.AddBaseReceiveHandle((client, pid, len) =>
            {
                if (!InputPacketBuffer.IsSystemPID(pid))
                    Logger.AppendInfo($"Receive packet {pid}");
            });

            return builder;
        }

        bool _signResult = false;

        bool signResult { get => _signResult; set { _signResult = value; OnStateChanged(State); } }


        bool initialized = false;

        public async void Initialize()
        {
            if (initialized)
                return;

            initialized = true; 

            await InitNetwork();
        }

        protected abstract Task<bool> InitNetwork();

        private async Task<RoomSignInResponseModel> TrySign()
        {
            var output = RequestPacketBuffer.Create(NodeBridgeRoomPacketEnum.SignServerRequest);

            new RoomSignInRequestModel()
            {
                Identity = ServerIdentity,
                ConnectionEndPoint = publicEndPoint,
                IdentityKey = identityKey
            }.WriteFullTo(output);

            _signResult = false;

            RoomSignInResponseModel response = default;

            await PacketWaitBuffer.SendRequestAsync(output, data =>
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
            var output = RequestPacketBuffer.Create(NodeBridgeRoomPacketEnum.SignSessionRequest);

            data.WriteFullTo(output);

            RoomSignSessionResponseModel result = default;

            await PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                result = RoomSignSessionResponseModel.ReadFullFrom(data);

                return Task.CompletedTask;
            });

            return result ?? new RoomSignSessionResponseModel() { Result = false };
        }

        internal async Task<RoomSignSessionPlayerResponseModel> TrySignSessionPlayer(RoomSignSessionPlayerRequestModel data)
        {
            var output = RequestPacketBuffer.Create(NodeBridgeRoomPacketEnum.SignSessionPlayerRequest);

            data.WriteFullTo(output);

            RoomSignSessionPlayerResponseModel result = default;

            await PacketWaitBuffer.SendRequestAsync(output, data =>
            {
                result = RoomSignSessionPlayerResponseModel.ReadFullFrom(data);

                return Task.CompletedTask;
            });

            return result ?? new RoomSignSessionPlayerResponseModel();
        }

        internal void FinishRoom(RoomInfo room, byte[] data)
        {
            var output = OutputPacketBuffer.Create(NodeBridgeRoomPacketEnum.FinishRoomMessage);

            output.WriteGuid(room.SessionId);

            if (data != null)
                output.Write(data);

            network.Network?.Send(output);

        }

        internal void RoomMessage(RoomInfo room, byte[] data)
        {
            var output = OutputPacketBuffer.Create(NodeBridgeRoomPacketEnum.RoomMessage);

            output.WriteGuid(room.SessionId);

            if (data != null)
                output.Write(data);

            network.Network?.Send(output);

        }

        public event Action<bool> OnStateChanged = (state) => { };

        public bool State => network?.GetState() == true && signResult;

        public bool IdentityFailed { get; private set; }
    }
}