using NSL.Logger.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer
{
    public class DefaultBridgeServerEntry : BridgeServerEntry<DefaultBridgeServerEntry>
    {
        internal DefaultBridgeServerEntry() { }
        private BridgeConfigurationManager configuration;
        private ILogger logger;

        public override BridgeConfigurationManager Configuration => configuration;

        public override ILogger Logger => logger;

        public override void Run()
        {
            logger = CreateConsoleLogger();

            configuration = CreateDefaultConfigurationManager();

            base.CreateDefaultLobbyServerNetwork();
            base.CreateDefaultTransportServerNetwork();
            base.CreateDefaultClientServerNetwork();
        }

    }
}
