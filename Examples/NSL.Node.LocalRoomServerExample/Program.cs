namespace NSL.Node.LocalRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseWebSockets();

            new LocalAspRoomServerStartupEntry(app, "/room_server").Run();

            app.Run();
        }
    }
}