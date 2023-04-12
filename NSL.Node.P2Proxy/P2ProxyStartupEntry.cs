using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.P2Proxy.Client;
using NSL.UDP.Info;
using STUN;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NSL.Node.P2Proxy
{
    public abstract class P2ProxyStartupEntry<TTHIS> : RoomServerStartupEntry
        where TTHIS : RoomServerStartupEntry
    {
        /// <summary>
        /// Call Run and return this
        /// </summary>
        /// <returns></returns>
        public P2ProxyStartupEntry<TTHIS> RunEntry() { Run(); return this; }
    }

    public abstract class RoomServerStartupEntry
    {
        public abstract P2ProxyConfigurationManager Configuration { get; }

        public abstract ILogger Logger { get; }

        public P2ProxyEntry RoomServer { get; protected set; }

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

        protected P2ProxyConfigurationManager CreateDefaultConfigurationManager()
            => new P2ProxyConfigurationManager(Logger);

        protected ILogger CreateConsoleLogger()
            => ConsoleLogger.Create();

        protected virtual P2ProxyEntry CreateClientServerNetwork()
            => RoomServer = P2ProxyEntry
                .Create(this)
                .Run();

        public static DefaultP2ProxyStartupEntry CreateDefault()
            => new DefaultP2ProxyStartupEntry();
    }
}
