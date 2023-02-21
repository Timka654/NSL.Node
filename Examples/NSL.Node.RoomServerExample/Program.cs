using NSL.Node.RoomServer;

namespace NSL.Node.BridgeTransportExample
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
            RoomServerStartupEntry.CreateDefault().RunEntry();

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}
