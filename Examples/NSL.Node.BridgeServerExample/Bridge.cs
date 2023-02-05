using NSL.ConfigurationEngine;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.Node.BridgeServer.CS;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.TS;
using NSL.Node.BridgeServer;

namespace NSL.Node.BridgeServerExample
{
    public class Bridge : BridgeServerEntry
    {
        private BaseConfigurationManager configuration;
        private ILogger logger;

        public override BaseConfigurationManager Configuration => configuration;

        public override ILogger Logger => logger;

        public override void Run()
        {
            logger = ConsoleLogger.Create();
            configuration = new ConfigurationManager(Logger);

            var lobbyServer = LobbyServer
                .Create(this)
                .Run();

            var transportServer = TransportServer
                .Create(this)
                .Run();

            ClientServer
                .Create(this, lobbyServer, transportServer)
                .Run();
        }
    }
}
