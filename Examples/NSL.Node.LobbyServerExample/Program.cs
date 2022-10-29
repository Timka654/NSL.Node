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
                builder.Configuration.GetValue("bridge:server:url", "ws://localhost:6999"),
                builder.Configuration.GetValue("bridge:server:identity", "270E1B1E-4889-4D46-8B9D-9325404FFD69"),
                builder.Configuration.GetValue("bridge:server:key", "270E1B1E-4889-4D46-8B9D-9325404FFD69"),
                (services, builder) => {

                });

            var app = builder.Build();

            app.UseWebSockets();

            app.MapWebSocketsPoint<LobbyNetworkClientModel>("/lobby_ws", builder =>
            {
                app.Services.GetRequiredService<LobbyManager>().BuildNetwork(builder);
            });

            app.UseRouting();

            app.RunBridgeLobbyClient(app.Services.GetRequiredService<LobbyManager>().BridgeValidateSessionAsync);

            app.Run();
        }
    }
}