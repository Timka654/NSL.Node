using NSL.Logger.Interface;
using NSL.Node.RoomServer.Client.Data;
using NSL.UDP.Info;
using NSL.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer
{
    public class DefaultRoomServerStartupEntry : RoomServerStartupEntry<DefaultRoomServerStartupEntry>
    {
        private RoomConfigurationManager configuration;
        private ILogger logger;

        public override RoomConfigurationManager Configuration => configuration;

        public override ILogger Logger => logger;

        static StunServerInfo[] defaultStunServers = new[]
        {
            new StunServerInfo("stun.l.google.com:19302"),
            new StunServerInfo("stun1.l.google.com:19302"),
            new StunServerInfo("stun2.l.google.com:19302"),
            new StunServerInfo("stun3.l.google.com:19302"),
            new StunServerInfo("stun4.l.google.com:19302"),
        };

        public override void Run()
        {
            logger = CreateConsoleLogger();

            configuration = CreateDefaultConfigurationManager();

            string endPoint = ClientPublicPoint;

            if (string.IsNullOrEmpty(endPoint))
            {
                if (!StunAutoDetect)
                    throw new Exception($"must be set client_public_endpoint or client_stun_detect(true value) for start");

                endPoint = BuildClientPublicPoint(GetStunAddress(defaultStunServers));
            }

            var bridge = base.CreateBridgeClientNetwork();

            base.CreateClientServerNetwork(bridge);

            bridge.OnStateChanged += BridgeNetwork_OnStateChanged;

            bridge.Authorize(endPoint);
        }

        bool initialized = false;

        protected virtual void BridgeNetwork_OnStateChanged(bool state)
        {
            if (initialized)
                return;

            if (BridgeClient.IdentityFailed)
                throw new Exception($"Cannot identity on bridge server!!");

            if (!state)
                throw new Exception($"Cannot connect to bridge server!!");

            initialized = true;
        }
    }
}
