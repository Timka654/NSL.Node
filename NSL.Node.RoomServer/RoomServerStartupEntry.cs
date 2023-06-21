using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client;
using NSL.Node.RoomServer.Client.Data;
using NSL.UDP.Info;
using STUN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer
{
    public abstract class RoomServerStartupEntry<TTHIS> : RoomServerStartupEntry
        where TTHIS : RoomServerStartupEntry
    {
        /// <summary>
        /// Call Run and return this
        /// </summary>
        /// <returns></returns>
        public RoomServerStartupEntry<TTHIS> RunEntry() { Run(); return this; }
    }

    public abstract class RoomServerStartupEntry
    {
        public abstract RoomConfigurationManager Configuration { get; }

        public abstract ILogger Logger { get; }

        public BridgeRoomNetwork BridgeClient { get; protected set; }

        public ClientServerEntry RoomServer { get; protected set; }

        public string ClientPublicPoint => Configuration.GetValue<string>("client_public_endpoint", default(string));

        public bool StunAutoDetect => Configuration.GetValue<bool>("client_stun_detect", default(bool));

        protected string GetStunAddress(StunServerInfo[] stunServers)
        {
            STUNQueryResult stunResult = default;

            STUNClient.ReceiveTimeout = 700;

            foreach (var item in stunServers)
            {
                var addr = Dns.GetHostAddresses(item.Address).OrderByDescending(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();

                stunResult = STUNClient.Query(new System.Net.IPEndPoint(addr, item.Port), STUNQueryType.ExactNAT, true);

                if (stunResult.QueryError == STUNQueryError.Success)
                    break;
            }

            if (stunResult?.QueryError != STUNQueryError.Success)
                throw new Exception($"Not found or error received from all stun servers");

            return stunResult.PublicEndPoint.Address.ToString();
        }

        protected string BuildClientPublicPoint(string address)
            => $"ws://{address}:{RoomServer.ClientBindingPort}/";

        public abstract void Run();

        protected virtual RoomConfigurationManager CreateDefaultConfigurationManager()
            => new RoomConfigurationManager(Logger);

        protected ILogger CreateConsoleLogger()
            => ConsoleLogger.Create();

        protected virtual BridgeRoomNetwork CreateBridgeClientNetwork()
            => BridgeClient = BridgeRoomNetwork
                .Create(this)
                .Run();

        protected virtual ClientServerEntry CreateClientServerNetwork(BridgeRoomNetwork bridgeClient)
            => RoomServer = ClientServerEntry
                .Create(this, bridgeClient)
                .Run();
        protected virtual ClientServerEntry CreateAspClientServerNetwork(BridgeRoomNetwork bridgeClient, IEndpointRouteBuilder builder, string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConvertionBuilder = null)
            => RoomServer = ClientServerEntry
                .Create(this, bridgeClient)
                .RunAsp(builder, pattern, requestHandle, actionConvertionBuilder);


        public virtual Task<RoomSignSessionResponseModel> ValidateSession(RoomSignSessionRequestModel request) => BridgeClient.TrySignSession(request);

        public virtual Task<RoomSignSessionPlayerResponseModel> ValidateSessionPlayer(RoomSignSessionPlayerRequestModel request) => BridgeClient.TrySignSessionPlayer(request);

        public virtual void FinishRoomHandle(RoomInfo room, byte[] data) => BridgeClient.FinishRoom(room,data);

        public virtual void RoomMessageHandle(RoomInfo room, byte[] data) => BridgeClient.RoomMessage(room,data);

        public static DefaultRoomServerStartupEntry CreateDefault()
            => new DefaultRoomServerStartupEntry();

        public virtual Task<string> GetProxyRoomId(RoomInfo roomInfo) => Task.FromResult(string.Empty);

        public virtual Task<IEnumerable<string>> GetProxyEndPoints() => Task.FromResult(Enumerable.Empty<string>());
    }
}
