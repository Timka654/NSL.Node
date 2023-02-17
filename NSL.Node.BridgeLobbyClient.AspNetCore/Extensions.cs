using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Logger.AspNetCore;
using NSL.WebSockets.Client;
using System;

namespace NSL.Node.BridgeLobbyClient.AspNetCore
{
    public static class Extensions
    {
        /// <summary>
        /// Add <see cref="BridgeLobbyNetwork"/> to service list
        /// </summary>
        /// <param name="services"></param>
        /// <param name="url"></param>
        /// <param name="serverIdentity"></param>
        /// <param name="identityKey"></param>
        /// <param name="onBuild"></param>
        /// <returns></returns>
        public static IServiceCollection AddBridgeLobbyClient(
            this IServiceCollection services,
            string url,
            string serverIdentity,
            string identityKey,
            Action<IServiceProvider, WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null
            )
        {
            services.AddSingleton<BridgeLobbyNetwork>(services => new BridgeLobbyNetwork(new System.Uri(url), serverIdentity, identityKey, builder =>
            {
                builder.SetLogger(new ILoggerWrapper(services.GetRequiredService<ILogger<BridgeLobbyNetwork>>()));

                if (onBuild != null)
                    onBuild(services, builder);
            }));

            return services;
        }

        public static void RunBridgeLobbyClient(this IHost host, ValidateSessionDelegate validateSession, RoomStartupInfoDelegate roomStartupInfo, RoomFinishDelegate roomFinish)
            => RunBridgeLobbyClient<BridgeLobbyNetwork>(host, validateSession, roomStartupInfo, roomFinish);

        public static void RunBridgeLobbyClient<TNetwork>(this IHost host, ValidateSessionDelegate validateSession, RoomStartupInfoDelegate roomStartupInfo, RoomFinishDelegate roomFinish)
            where TNetwork : BridgeLobbyNetwork
        {
            var network = host.Services.GetRequiredService<TNetwork>();

            network.ValidateSession = validateSession;
            network.RoomStartupInfo = roomStartupInfo;
            network.RoomFinish = roomFinish;

            network.Initialize();
        }
    }
}
