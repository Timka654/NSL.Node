using NSL.Node.RoomServer;
using NSL.Node.RoomServer.Client.Data;

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

            ExampleRoomServerStartupEntry.CreateDefault().RunEntry();

            Console.WriteLine("Success initialized");

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }

    public class ExampleRoomServerStartupEntry: DefaultRoomServerStartupEntry
    {
        public override Task<string> GetProxyRoomId(RoomInfo roomInfo) => Task.FromResult($"{roomInfo.SessionId}");

        public override Task<IEnumerable<string>> GetProxyEndPoints() => Task.FromResult(Enumerable.Repeat("udp://localhost:5980", 1));

        public static ExampleRoomServerStartupEntry CreateDefault()
            => new ExampleRoomServerStartupEntry();
    }
}
