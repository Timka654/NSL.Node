using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeServer.CS;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Managers;
using NSL.Node.BridgeServer.RS;

namespace NSL.Node.BridgeServer
{
    public abstract class BridgeServerStartupEntry<TTHIS> : BridgeServerStartupEntry
        where TTHIS : BridgeServerStartupEntry
    {
        /// <summary>
        /// Call Run and return this
        /// </summary>
        /// <returns></returns>
        public BridgeServerStartupEntry<TTHIS> RunEntry() { Run(); return this; }
    }

    public abstract class BridgeServerStartupEntry
    {
        public abstract BridgeConfigurationManager Configuration { get; }

        public abstract ILogger Logger { get; }

        public LobbyServerEntry LobbyServer { get; protected set; }

        public RoomServerEntry TransportServer { get; protected set; }

        public ClientServerEntry ClientServer { get; protected set; }

        internal RoomManager RoomManager { get; }

        internal LobbyManager LobbyManager { get; }

        public BridgeServerStartupEntry()
        {
            RoomManager = new RoomManager(this);
            LobbyManager = new LobbyManager(this);
        }


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
                .Create(this)
                .Run();

        public static DefaultBridgeServerStartupEntry CreateDefault()
            => new DefaultBridgeServerStartupEntry();
    }
}
