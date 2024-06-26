﻿using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client;
using NSL.Node.RoomServer.Data;
using NSL.UDP.Info;
using STUN;
using System;
using System.Linq;
using System.Net;
using System.Runtime.Loader;

namespace NSL.Node.RoomServer
{
    public class NodeRoomServerEntryBuilder
    {
        public static NodeRoomServerEntryBuilder Create() => new NodeRoomServerEntryBuilder();

        public NodeRoomServerEntry Entry { get; } = new NodeRoomServerEntry();

        private bool processed;

        public NodeRoomServerEntry Run()
        {
            Entry.Run();

            processed = true;

            return Entry;
        }

        public NodeRoomServerEntryBuilder WithHandleProcessor(RoomServerHandleProcessor value)
            => WithBridgeStateChangedHandle(value.OnBridgeStateChangeHandle)
                .WithValidateSessionHandle(value.ValidateSession)
                .WithValidateSessionPlayerHandle(value.ValidateSessionPlayer)
                .WithRoomMessageHandle(value.RoomMessageHandle)
                .WithRoomFinishHandle(value.FinishRoomHandle);

        public NodeRoomServerEntryBuilder WithCreateSessionHandle(NodeRoomServerEntry.CreateSessionDelegate value)
        {
            Entry.CreateRoomSession = value;

            return this;
        }

        public NodeRoomServerEntryBuilder WithBridgeStateChangedHandle(NodeRoomServerEntry.StateChangeDelegate value)
        {
            Entry.BridgeConnectionStateChangedHandle = value;

            return this;
        }

        public NodeRoomServerEntryBuilder WithValidateSessionHandle(NodeRoomServerEntry.ValidateSessionDelegate value)
        {
            Entry.ValidateSession = value;

            return this;
        }

        public NodeRoomServerEntryBuilder WithValidateSessionPlayerHandle(NodeRoomServerEntry.ValidateSessionPlayerDelegate value)
        {
            Entry.ValidateSessionPlayer = value;

            return this;
        }

        public NodeRoomServerEntryBuilder WithRoomMessageHandle(NodeRoomServerEntry.RoomMessageHandleDelegate value)
        {
            Entry.RoomMessageHandle = value;

            return this;
        }

        public NodeRoomServerEntryBuilder WithRoomFinishHandle(NodeRoomServerEntry.RoomFinishHandleDelegate value)
        {
            Entry.RoomFinishHandle = value;

            return this;
        }

        public NodeRoomServerEntryBuilder WithDebugPacketIO(bool active = true)
        {
            Entry.DebugPacketIO = active;

            return this;
        }

        public NodeRoomServerEntryBuilder WithLogger(ILogger logger)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.Logger = logger;

            return this;
        }

        public NodeRoomServerEntryBuilder WithReconnectSessionLifeTime(TimeSpan lifetime)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.ReconnectSessionLifeTime = lifetime;

            return this;
        }

        public NodeRoomServerEntryBuilder WithClientServerListener(ClientServerBaseEntry entry)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.ClientServerListener = entry;

            return this;
        }

        public NodeRoomServerEntryBuilder WithBridgeRoomClient(BridgeRoomBaseNetwork entry)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            Entry.BridgeNetworkClient = entry;

            return this;
        }

        public NodeRoomServerEntryBuilder WithConsoleLogger()
            => WithLogger(new ConsoleLogger());

        public NodeRoomServerEntryBuilder GetPublicAddressFromStun(int port, bool isHttps, out string address, StunServerInfo[] stunServers = null)
        {
            GetPublicAddressFromStun(out address, stunServers);

            if (address != default)
                address = $"ws{(isHttps ? "s" : "")}://{address}:{port}/";

            return this;
        }

        public NodeRoomServerEntryBuilder GetPublicAddressFromStun(out string address, StunServerInfo[] stunServers = null)
        {
            STUNQueryResult stunResult = default;

            STUNClient.ReceiveTimeout = 700;

            stunServers ??= defaultStunServers;

            foreach (var item in stunServers)
            {
                var addr = Dns.GetHostAddresses(item.Address).OrderByDescending(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();

                stunResult = STUNClient.Query(new System.Net.IPEndPoint(addr, item.Port), STUNQueryType.ExactNAT, true);

                if (stunResult.QueryError == STUNQueryError.Success)
                    break;
            }

            if (stunResult?.QueryError != STUNQueryError.Success)
                throw new Exception($"Not found or error received from all stun servers");

            address = stunResult.PublicEndPoint.Address.ToString();

            return this;
        }

        static StunServerInfo[] defaultStunServers = new[]
        {
            new StunServerInfo("stun.l.google.com:19302"),
            new StunServerInfo("stun1.l.google.com:19302"),
            new StunServerInfo("stun2.l.google.com:19302"),
            new StunServerInfo("stun3.l.google.com:19302"),
            new StunServerInfo("stun4.l.google.com:19302"),
        };
    }
}
