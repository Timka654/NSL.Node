using NSL.Node.RoomServer;

namespace NSL.Node.AspRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var x = new RoomConfigurationManager(null);
            foreach (var r in x.GetAllValues())
            {
                Console.WriteLine($"{r.Path}  ::  {r.Value}");
            }
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            AspRoomServerStartupEntry.Create(app, "/room_server").Run();

            app.Run();
        }
    }
}