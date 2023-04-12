using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Providers;
using NSL.ConfigurationEngine.Providers.Json;
using NSL.Logger.Interface;
using System;

namespace NSL.Node.P2Proxy
{
    public class P2ProxyConfigurationManager : BaseConfigurationManager
    {
        public P2ProxyConfigurationManager(ILogger logger) : this(logger, "configuration.json")
        { }

        public P2ProxyConfigurationManager(ILogger logger, string configurationJsonFilePath) : this(logger, manager =>
        {
            manager.AddProvider(new JsonConfigurationProvider(configurationJsonFilePath, false, true));
            manager.AddProvider(new EnvironmentVariableConfigurationProvider());
            manager.AddProvider(new CommandLineArgsConfigurationProvider());
        })
        { }

        public P2ProxyConfigurationManager(ILogger logger, Action<P2ProxyConfigurationManager> beforeLoading)
        {
            if (logger != null)
                OnLog += logger.Append;

            beforeLoading(this);

            ReloadData();
        }
    }
}
