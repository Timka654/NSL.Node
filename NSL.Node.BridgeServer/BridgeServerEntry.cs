using NSL.ConfigurationEngine;
using NSL.Logger.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer
{
    public abstract class BridgeServerEntry
    {
        public abstract BaseConfigurationManager Configuration { get; }

        public abstract ILogger Logger { get; }

        public abstract void Run();
    }
}
