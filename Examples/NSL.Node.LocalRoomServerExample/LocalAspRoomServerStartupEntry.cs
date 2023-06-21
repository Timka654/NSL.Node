using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.Node.RoomServer;
using NSL.Node.RoomServer.Bridge;
using NSL.Logger.Interface;
using ILogger = NSL.Logger.Interface.ILogger;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;

namespace NSL.Node.LocalRoomServerExample
{
    public class LocalAspRoomServerStartupEntry : AspRoomServerStartupEntry
    {
        private readonly IEndpointRouteBuilder builder;
        private readonly string clientPattern;

        private RoomConfigurationManager configuration;
        private ILogger logger;


        public override RoomConfigurationManager Configuration => configuration;

        public override ILogger Logger => logger;

        public LocalAspRoomServerStartupEntry(IEndpointRouteBuilder builder, string clientPattern) : base(builder, clientPattern)
        {
            this.builder = builder;
            this.clientPattern = clientPattern;
        }

        protected override BridgeRoomNetwork CreateBridgeClientNetwork()
        {
            return null;
        }

        public override Task<RoomSignSessionResponseModel> ValidateSession(RoomSignSessionRequestModel request)
        {
            var options = new NodeRoomStartupInfo();

            options.SetRoomNodeCount(1);
            options.SetRoomWaitReady(false);

            options.SetDestroyOnEmpty(true);

            return Task.FromResult(new RoomSignSessionResponseModel() { Result = true, RoomId = request.RoomIdentity, Options = options.GetDictionary() });
        }

        public override Task<RoomSignSessionPlayerResponseModel> ValidateSessionPlayer(RoomSignSessionPlayerRequestModel request)
        {
            return Task.FromResult(new RoomSignSessionPlayerResponseModel() { ExistsSession = true, ExistsPlayer = true });
        }

        public override void FinishRoomHandle(RoomInfo room, byte[] data)
        {
            // ignore on local
        }

        public override void RoomMessageHandle(RoomInfo room, byte[] data)
        {
            // ignore on local
        }

        public override void Run()
        {
            //base.Run();

            logger = CreateConsoleLogger();
            base.CreateAspClientServerNetwork(null, builder, clientPattern);
        }
    }
}
