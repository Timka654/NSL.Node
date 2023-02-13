using NSL.ConfigurationEngine;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeServer.CS;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.RS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer
{
    public abstract class BridgeServerEntry<TTHIS> : BridgeServerEntry
        where TTHIS : BridgeServerEntry
    {
        /// <summary>
        /// Call Run and return this
        /// </summary>
        /// <returns></returns>
        public BridgeServerEntry<TTHIS> RunEntry() { Run(); return this; }
    }

    public abstract class BridgeServerEntry
    {
        public abstract BridgeConfigurationManager Configuration { get; }

        public abstract ILogger Logger { get; }

        public LobbyServerEntry LobbyServer { get; protected set; }

        public RoomServerEntry TransportServer { get; protected set; }

        public ClientServerEntry ClientServer { get; protected set; }


        public abstract void Run();

        protected BridgeConfigurationManager CreateDefaultConfigurationManager()
            =>  new BridgeConfigurationManager(Logger);

        protected ILogger CreateConsoleLogger()
            => ConsoleLogger.Create();

        protected virtual LobbyServerEntry CreateDefaultLobbyServerNetwork()
            => LobbyServer = LobbyServerEntry
                .Create(this)
                .Run();

        protected virtual RoomServerEntry CreateDefaultTransportServerNetwork()
            => TransportServer = RoomServerEntry
                .Create(this)
                .Run();

        protected virtual ClientServerEntry CreateDefaultClientServerNetwork()
            => ClientServer = ClientServerEntry
                .Create(this, LobbyServer, TransportServer)
                .Run();

        public static DefaultBridgeServerEntry CreateDefault()
            => new DefaultBridgeServerEntry();
    }
}
