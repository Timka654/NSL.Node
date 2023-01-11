using Microsoft.EntityFrameworkCore;
using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using WelcomeServer.Data;
using WelcomeServer.Data.Models;
using WelcomeServer.Data.Repositories;
using WelcomeServer.Managers;
using WelcomeServer.Managers.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("default")));

builder.Services.AddSingleton<LobbyManager>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
