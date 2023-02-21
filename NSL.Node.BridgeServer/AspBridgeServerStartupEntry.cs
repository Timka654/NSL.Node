using Microsoft.AspNetCore.Routing;
using NSL.Logger.Interface;

namespace NSL.Node.BridgeServer
{
    public class AspBridgeServerStartupEntry : BridgeServerStartupEntry<DefaultBridgeServerStartupEntry>
    {
        internal AspBridgeServerStartupEntry() { }

        public AspBridgeServerStartupEntry(IEndpointRouteBuilder builder, string lobbyPattern, string roomPattern, string clientPattern)
        {
            this.builder = builder;
            this.lobbyPattern = lobbyPattern;
            this.roomPattern = roomPattern;
            this.clientPattern = clientPattern;
        }

        private BridgeConfigurationManager configuration;
        private ILogger logger;
        private IEndpointRouteBuilder builder;
        private string lobbyPattern;
        private string roomPattern;
        private string clientPattern;

        public override BridgeConfigurationManager Configuration => configuration;

        public override ILogger Logger => logger;

        public override void Run()
        {
            logger = CreateConsoleLogger();

            configuration = CreateDefaultConfigurationManager();

            base.CreateAspLobbyServerNetwork(builder, lobbyPattern);
            base.CreateAspRoomServerNetwork(builder, roomPattern);
            base.CreateAspClientServerNetwork(builder, clientPattern);
        }

        public static AspBridgeServerStartupEntry Create(IEndpointRouteBuilder builder, string lobbyPattern, string roomPattern, string clientPattern) 
            => new AspBridgeServerStartupEntry(builder, lobbyPattern, roomPattern, clientPattern);
    }
}
