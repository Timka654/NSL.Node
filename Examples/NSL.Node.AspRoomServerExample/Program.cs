using NSL.Node.RoomServer.AspNetCore;
using NSL.Node.RoomServer.Bridge;

namespace NSL.Node.AspRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddNodeRoomServer();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseWebSockets();

            app.RunNodeRoomServer(c => c
            .WithDebugPacketIO(true)
            .WithAspLogger(app.Logger)
            .WithBridgeDefaultHandles()
            //.WithCreateSessionHandle(roomInfo=> new GameInfo(roomInfo))
            //.GetPublicAddressFromStun(out var publicAddr)
            .WithRoomBridgeNetwork("wss://localhost:7023/room_server", new Dictionary<string, string>(), "wss://localhost")
            .WithClientServerAspBinding(app, "/room_server")
            );

            app.Run();
        }
    }
}