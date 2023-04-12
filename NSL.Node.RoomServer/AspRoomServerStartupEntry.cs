using Microsoft.AspNetCore.Routing;
using NSL.Logger.Interface;
using NSL.UDP.Info;
using System;

namespace NSL.Node.RoomServer
{
    public class AspRoomServerStartupEntry : RoomServerStartupEntry<DefaultRoomServerStartupEntry>
    {
        internal AspRoomServerStartupEntry() { }

        public AspRoomServerStartupEntry(IEndpointRouteBuilder builder, string clientPattern)
        {
            this.builder = builder;
            this.clientPattern = clientPattern;
        }


        private RoomConfigurationManager configuration;
        private ILogger logger;
        private IEndpointRouteBuilder builder;
        private string clientPattern;

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
                    throw new Exception($"must be set transport.client_public_endpoint or client_stun_detect(true value) for start");

                endPoint = BuildClientPublicPoint(GetStunAddress(defaultStunServers));
            }

            var bridge = base.CreateBridgeClientNetwork();

            base.CreateAspClientServerNetwork(bridge, builder, clientPattern);

            bridge.OnStateChanged += BridgeNetwork_OnStateChanged;

            bridge.Authorize(endPoint);
        }

        bool initialized = false;

        private void BridgeNetwork_OnStateChanged(bool state)
        {
            if (initialized)
                return;

            if(BridgeClient.IdentityFailed)
                throw new Exception($"Identity data failed!!");

            if (!state)
                throw new Exception($"State of Bridge network is invalid = {state}!!");

            initialized = true;
        }

        public static AspRoomServerStartupEntry Create(IEndpointRouteBuilder builder, string clientPattern)
            => new AspRoomServerStartupEntry(builder, clientPattern);
    }
}
