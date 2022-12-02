using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.BridgeTransportClient.Bridge;
using NSL.Node.BridgeTransportClient.Transport;
using NSL.UDP.Client.Info;
using NSL.WebSockets.Client;
using NSL.WebSockets.Server;
using STUN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportExample
{
    public class Program
    {
        static BridgeTransportNetwork BridgeNetwork;

        static TransportNetwork TransportNetwork;

        static StunServerInfo[] stunServers = new[]
        {
            new StunServerInfo("stun.l.google.com:19302"),
            new StunServerInfo("stun1.l.google.com:19302"),
            new StunServerInfo("stun2.l.google.com:19302"),
            new StunServerInfo("stun3.l.google.com:19302"),
            new StunServerInfo("stun4.l.google.com:19302"),
        };

        internal static ILogger Logger { get; } = new ConsoleLogger();

        internal static ConfigurationManager Configuration { get; } = new ConfigurationManager();

        internal static int BindingPort => Configuration.GetValue("transport.binding.port", 5920);
        internal static string PublicPoint => Configuration.GetValue("transport.public.endpoint", default(string));


        public static void Main(string[] args)
        {
            string publicPoint = PublicPoint;

            if (publicPoint == default)
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

                publicPoint = stunResult.PublicEndPoint.Address.ToString();

                publicPoint = $"http://{publicPoint}:{BindingPort}/";
            }

            BridgeNetwork = new BridgeTransportNetwork(new Uri($"ws://localhost:6998"), "AABBCC", c =>
            {
                c.SetLogger(Logger);

                c.AddDefaultEventHandlers<WebSocketsClientEndPointBuilder<BridgeTransportNetworkClient, WSClientOptions<BridgeTransportNetworkClient>>, BridgeTransportNetworkClient>("[Bridge]", DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);
            });

            BridgeNetwork.OnStateChanged += BridgeNetwork_OnStateChanged;

            TransportNetwork = new TransportNetwork(BridgeNetwork, $"http://*:{BindingPort}/", c =>
            {
                c.SetLogger(Logger);

                c.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<TransportNetworkClient, WSServerOptions<TransportNetworkClient>>, TransportNetworkClient>("[Transport]", DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);
            });

            BridgeNetwork.Initialize(publicPoint);

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }

        static bool initialized = false;

        private static void BridgeNetwork_OnStateChanged(bool state)
        {
            if (initialized)
                return;

            if (!state)
                throw new Exception($"Cannot identity!!");

            initialized = true;
        }
    }
}
