using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeServer.CS;
using NSL.Node.BridgeServer.LS;
using NSL.Node.BridgeServer.Managers;
using NSL.Node.BridgeServer.RS;
using System.Threading.Tasks;
using System;

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

        public RoomServerEntry RoomServer { get; protected set; }

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
            => new BridgeConfigurationManager(Logger);

        protected ILogger CreateConsoleLogger()
            => ConsoleLogger.Create();

        protected virtual LobbyServerEntry CreateDefaultLobbyServerNetwork()
            => LobbyServer = LobbyServerEntry
                .Create(this)
                .Run();

        protected virtual RoomServerEntry CreateDefaultRoomServerNetwork()
            => RoomServer = RoomServerEntry
                .Create(this)
                .Run();

        protected virtual ClientServerEntry CreateDefaultClientServerNetwork()
            => ClientServer = ClientServerEntry
                .Create(this)
                .Run();

        protected virtual LobbyServerEntry CreateAspLobbyServerNetwork(IEndpointRouteBuilder builder, string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConvertionBuilder = null)
            => LobbyServer = LobbyServerEntry
                .Create(this)
                .RunAsp(builder, pattern, requestHandle, actionConvertionBuilder);

        protected virtual RoomServerEntry CreateAspRoomServerNetwork(IEndpointRouteBuilder builder, string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConvertionBuilder = null)
            => RoomServer = RoomServerEntry
                .Create(this)
                .RunAsp(builder, pattern, requestHandle, actionConvertionBuilder);

        protected virtual ClientServerEntry CreateAspClientServerNetwork(IEndpointRouteBuilder builder, string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConvertionBuilder = null)
            => ClientServer = ClientServerEntry
                .Create(this)
                .RunAsp(builder, pattern, requestHandle, actionConvertionBuilder);


        public static DefaultBridgeServerStartupEntry CreateDefault()
            => new DefaultBridgeServerStartupEntry();
    }
}
