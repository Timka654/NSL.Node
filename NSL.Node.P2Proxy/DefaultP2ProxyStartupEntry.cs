using NSL.Logger.Interface;
using NSL.UDP.Info;
using System;

namespace NSL.Node.P2Proxy
{
    public class DefaultP2ProxyStartupEntry : P2ProxyStartupEntry<DefaultP2ProxyStartupEntry>
    {
        internal DefaultP2ProxyStartupEntry() { }
        private P2ProxyConfigurationManager configuration;
        private ILogger logger;

        public override P2ProxyConfigurationManager Configuration => configuration;

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

            base.CreateClientServerNetwork();
        }
    }
}
