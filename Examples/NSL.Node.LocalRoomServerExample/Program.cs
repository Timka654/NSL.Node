using NSL.Node.RoomServer.AspNetCore;

namespace NSL.Node.LocalRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddNodeRoomServer();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseWebSockets();

            app.RunNodeRoomServer(c => c
            .WithAspLogger(app.Logger)
            .WithHandleProcessor(new LocalAspRoomServerStartupEntry())
            .GetPublicAddressFromStun(out var publicAddr)
            .WithClientServerAspBinding(app, "/room_server")
            );

            app.Run();
        }
    }
}