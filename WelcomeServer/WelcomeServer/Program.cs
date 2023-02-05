using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.Node.BridgeLobbyClient.AspNetCore;
using WelcomeServer.Data;
using WelcomeServer.Data.Models;
using WelcomeServer.Data.Repositories;
using WelcomeServer.Data.Repositories.Interfaces;
using WelcomeServer.Managers;
using WelcomeServer.Managers.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("default")));

builder.Services.AddBridgeLobbyClient(
           builder.Configuration.GetValue<string>("bridge:server:url"),
           builder.Configuration.GetValue<string>("bridge:server:identity"),
           builder.Configuration.GetValue<string>("bridge:server:key"),
           (services, builder) => {

           });
builder.Services.AddSingleton<LobbyManager>();
SetDependencies();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

app.MapWebSocketsPoint<LobbyNetworkClientModel>("/lobby_ws", builder =>
{
    builder.AddExceptionHandle((ex, c) =>
    {
        Console.WriteLine($"Exception {Environment.NewLine}{ex}{Environment.NewLine} from client");
    });

    builder.AddReceiveHandle((client, pid, len) =>
    {
        Console.WriteLine($"receive pid : {pid} from {client.GetRemotePoint()}");
    });

    builder.AddSendHandle((client, pid, len, stack) =>
    {
        Console.WriteLine($"send pid : {pid} to {client.GetRemotePoint()}");
    });

    app.Services.GetRequiredService<LobbyManager>().BuildNetwork(builder);
});
app.UseRouting();

app.RunBridgeLobbyClient(app.Services.GetRequiredService<LobbyManager>().BridgeValidateSessionAsync);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void SetDependencies()
{
    builder.Services.AddScoped<IUserManager, UserManager>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ICredentialsRepository, CredentialRepository>();
}