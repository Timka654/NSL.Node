using Microsoft.Extensions.DependencyInjection;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.Node.BridgeLobbyClient;
using NSL.Node.BridgeLobbyClient.AspNetCore;
using NSL.Node.LobbyServerExample.Managers;
using NSL.Node.LobbyServerExample.Shared.Models;

namespace NSL.Node.LobbyServerExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<LobbyManager>();

            builder.Services.AddBridgeLobbyClient(
                builder.Configuration.GetValue<string>("bridge:server:url"),
                builder.Configuration.GetValue<string>("bridge:server:identity"),
                builder.Configuration.GetValue<string>("bridge:server:key"),
                (services, builder) => {

                });

            var app = builder.Build();

            app.UseWebSockets();

            app.MapWebSocketsPoint<LobbyNetworkClientModel>("/lobby_ws", builder =>
            {
                builder.AddReceiveHandle((client, pid, len) =>
                {
                    app.Services.GetService<ILogger<LobbyNetworkClientModel>>().Log(LogLevel.Information, $"receive pid : {pid}");
                });
                app.Services.GetRequiredService<LobbyManager>().BuildNetwork(builder);
            });

            app.UseRouting();

            app.RunBridgeLobbyClient(app.Services.GetRequiredService<LobbyManager>().BridgeValidateSessionAsync);

            app.Run();
        }
    }
}