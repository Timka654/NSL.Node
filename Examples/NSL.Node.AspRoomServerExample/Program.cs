using NSL.Node.RoomServer.AspNetCore;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Shared.Client.Core;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace NSL.Node.AspRoomServerExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddNodeRoomServer();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseWebSockets();
#if DEBUG
            string bridgeConnectUrl = "ws://176.105.203.101:5208/room_server";

            int port = 7269;

            string publicAddr = $"wss://localhost:{port}/";
#else
            string bridgeConnectUrl = "wss://rectskullsapi.azurewebsites.net/room_server";

            string publicAddr = "wss://devroomserver2.azurewebsites.net/";

#endif
            //wss://localhost:57184/room_server
            //wss://localhost:57183/room_server

            app.RunNodeRoomServer(c => c
            .WithAspLogger(app.Logger)
            .WithBridgeDefaultHandles()
            .WithCreateSessionHandle(roomInfo => new GameInfo(roomInfo))
            //#if RELEASE
            //            .GetPublicAddressFromStun(port, true, out publicAddr)
            //#endif
            //.WithRoomBridgeNetwork("wss://localhost:7023/room_server", publicAddr, string.Empty)
            .WithRoomBridgeNetwork(bridgeConnectUrl, $"{publicAddr}room_server", string.Empty)
            .WithClientServerAspBinding(app, "/room_server")
            );

            app.Run();
        }
    }
}