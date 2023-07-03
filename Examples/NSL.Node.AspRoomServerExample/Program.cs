using NSL.Node.RoomServer.AspNetCore;
using NSL.Node.RoomServer.Bridge;

namespace NSL.Node.AspRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var x = new RoomConfigurationManager(null);
            //foreach (var r in x.GetAllValues())
            //{
            //    Console.WriteLine($"{r.Path}  ::  {r.Value}");
            //}
           var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddNodeRoomServer();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseWebSockets();

            app.RunNodeRoomServer(c => c
            .WithAspLogger(app.Logger)
            .WithBridgeDefaultHandles()
            .GetPublicAddressFromStun(out var publicAddr)
            .WithRoomBridgeNetwork("wss://localhost:7023/room_server", publicAddr, string.Empty)
            .WithClientServerAspBinding(app, "/room_server")
            );

            app.Run();
        }
    }
}