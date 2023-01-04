using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Providers;
using NSL.ConfigurationEngine.Providers.Json;

namespace NSL.Node.BridgeServer
{
    internal class ConfigurationManager : BaseConfigurationManager
    {
        public ConfigurationManager()
        {
            OnLog += Program.Logger.Append;

            AddProvider(new JsonConfigurationProvider("configuration.json", false, true));
            AddProvider(new EnvironmentVariableConfigurationProvider());

            ReloadData();
        }
    }
}
