using NSL.Node.RoomServer;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Data;

namespace NSL.Node.BridgeTransportExample
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

            //ExampleRoomServerStartupEntry.CreateDefault().RunEntry();

            int clientPort = 9999;

            NodeRoomServerEntryBuilder.Create()
                .WithConsoleLogger()
                .WithBridgeDefaultHandles()
                //.WithCreateSessionHandle(roomInfo=> new GameInfo(roomInfo))
                //.GetPublicAddressFromStun(clientPort,false, out var connectionPoint)
                .WithRoomBridgeNetwork("wss://localhost:7023/room_server", new Dictionary<string, string>(), "tcp://localhost:9999")
                .WithTCPClientServerBinding(clientPort)
                .Run();


            Console.WriteLine("Success initialized");

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}
