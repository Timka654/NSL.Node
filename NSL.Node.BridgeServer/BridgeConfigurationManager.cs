using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Providers;
using NSL.ConfigurationEngine.Providers.Json;
using NSL.Logger.Interface;

namespace NSL.Node.BridgeServer
{
    public class BridgeConfigurationManager : BaseConfigurationManager
    {
        public BridgeConfigurationManager(ILogger logger) : this(logger, "configuration.json")
        { }

        public BridgeConfigurationManager(ILogger logger, string configurationJsonFilePath) : this(logger, manager =>
        {
            manager.AddProvider(new EnvironmentVariableConfigurationProvider());
            manager.AddProvider(new JsonConfigurationProvider(configurationJsonFilePath, false, true));
            manager.AddProvider(new CommandLineArgsConfigurationProvider());
        })
        { }

        public BridgeConfigurationManager(ILogger logger, Action<BridgeConfigurationManager> beforeLoading)
        {
            if (logger != null)
                OnLog += logger.Append;

            beforeLoading(this);

            ReloadData();
        }
    }
}
