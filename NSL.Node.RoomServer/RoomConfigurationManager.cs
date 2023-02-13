using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Providers;
using NSL.ConfigurationEngine.Providers.Json;
using NSL.Logger.Interface;
using System;

namespace NSL.Node.RoomServer
{
    public class RoomConfigurationManager : BaseConfigurationManager
    {
        public RoomConfigurationManager(ILogger logger) : this(logger, "configuration.json")
        { }

        public RoomConfigurationManager(ILogger logger, string configurationJsonFilePath) : this(logger, manager =>
        {
            manager.AddProvider(new EnvironmentVariableConfigurationProvider());
            manager.AddProvider(new JsonConfigurationProvider(configurationJsonFilePath, false, true));
            manager.AddProvider(new CommandLineArgsConfigurationProvider());
        })
        { }

        public RoomConfigurationManager(ILogger logger, Action<RoomConfigurationManager> beforeLoading)
        {
            if (logger != null)
                OnLog += logger.Append;

            beforeLoading(this);

            ReloadData();
        }
    }
}
