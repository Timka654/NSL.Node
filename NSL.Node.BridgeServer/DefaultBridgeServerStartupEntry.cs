using NSL.Logger.Interface;

namespace NSL.Node.BridgeServer
{
    public class DefaultBridgeServerStartupEntry : BridgeServerStartupEntry<DefaultBridgeServerStartupEntry>
    {
        internal DefaultBridgeServerStartupEntry() { }
        private BridgeConfigurationManager configuration;
        private ILogger logger;

        public override BridgeConfigurationManager Configuration => configuration;

        public override ILogger Logger => logger;

        public override void Run()
        {
            logger = CreateConsoleLogger();

            configuration = CreateDefaultConfigurationManager();

            base.CreateDefaultLobbyServerNetwork();
            base.CreateDefaultRoomServerNetwork();
            base.CreateDefaultClientServerNetwork();
        }

        public static DefaultBridgeServerStartupEntry Create() => new DefaultBridgeServerStartupEntry();
    }
}
