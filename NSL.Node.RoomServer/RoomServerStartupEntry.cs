using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client;
using NSL.UDP.Client.Info;
using STUN;
using System;
using System.Linq;
using System.Net;

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

        public string BridgeIdentityKey => Configuration.GetValue<string>("bridge.identity.key", "AABBCC");

        public string BridgeAddress => Configuration.GetValue<string>("bridge.address", "ws://localhost:6998");


        public int ClientBindingPort => Configuration.GetValue<int>("client.binding.port", 5920);

        public string ClientPublicPoint => Configuration.GetValue<string>("client.public.endpoint", default(string));


        public bool StunAutoDetect => Configuration.GetValue<bool>("client.stun.detect", default(bool));

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
            => $"ws://{address}:{ClientBindingPort}/";

        public abstract void Run();

        protected RoomConfigurationManager CreateDefaultConfigurationManager()
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

        public static DefaultRoomServerStartupEntry CreateDefault()
            => new DefaultRoomServerStartupEntry();
    }
}
