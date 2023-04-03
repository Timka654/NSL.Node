using NSL.Logger.Interface;
using NSL.UDP.Info;
using System;

namespace NSL.Node.RoomServer
{
    public class DefaultRoomServerStartupEntry : RoomServerStartupEntry<DefaultRoomServerStartupEntry>
    {
        internal DefaultRoomServerStartupEntry() { }
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
                    throw new Exception($"must be set transport.public.endpoint or transport.stun.detect(true value) for start");

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

            if(BridgeClient.IdentityFailed)
                throw new Exception($"Cannot identity on bridge server!!");

            if (!state)
                throw new Exception($"Cannot connect to bridge server!!");

            initialized = true;
        }
    }
}
