using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Logger.AspNetCore;
using NSL.Node.BridgeLobbyClient.Models;
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
            Action<IServiceProvider, BridgeLobbyNetworkHandlesConfigurationModel> onHandleConfiguration,
            Action<IServiceProvider, WebSocketsClientEndPointBuilder<BridgeLobbyNetworkClient, WSClientOptions<BridgeLobbyNetworkClient>>> onBuild = null
            )
        {
            services.AddSingleton<BridgeLobbyNetwork>(services => new BridgeLobbyNetwork(
                new System.Uri(url),
                serverIdentity,
                identityKey,
                (handles) => onHandleConfiguration(services, handles),
                builder =>
            {
                builder.SetLogger(new ILoggerWrapper(services.GetRequiredService<ILogger<BridgeLobbyNetwork>>()));

                if (onBuild != null)
                    onBuild(services, builder);
            }));

            return services;
        }

        public static void RunBridgeLobbyClient(this IHost host)
            => RunBridgeLobbyClient<BridgeLobbyNetwork>(host);

        public static void RunBridgeLobbyClient<TNetwork>(this IHost host)
            where TNetwork : BridgeLobbyNetwork
        {
            var network = host.Services.GetRequiredService<TNetwork>();

            network.Initialize();
        }
    }
}
