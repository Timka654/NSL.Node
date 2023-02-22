using NSL.Node.RoomServer;

namespace NSL.Node.AspRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            app.UseHttpsRedirection();

            AspRoomServerStartupEntry.Create(app, "/room_server").Run();

            app.Run();
        }
    }
}