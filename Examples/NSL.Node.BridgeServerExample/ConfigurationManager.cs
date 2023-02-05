using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Providers;
using NSL.ConfigurationEngine.Providers.Json;
using NSL.Logger.Interface;

namespace NSL.Node.BridgeServer
{
    internal class ConfigurationManager : BaseConfigurationManager
    {
        public ConfigurationManager(ILogger logger)
        {
            if (logger != null)
                OnLog += logger.Append;

            AddProvider(new JsonConfigurationProvider("configuration.json", false, true));
            AddProvider(new EnvironmentVariableConfigurationProvider());

            ReloadData();
        }
    }
}
