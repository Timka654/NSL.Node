using NSL.Node.RoomServer;
using NSL.Node.RoomServer.Bridge;
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

            NodeRoomServerEntryBuilder.Create()
                .WithConsoleLogger()
                .WithBridgeDefaultHandles()
                .WithRoomBridgeNetwork("wss://localhost")


            Console.WriteLine("Success initialized");

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}
