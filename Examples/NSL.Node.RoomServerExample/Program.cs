using NSL.Node.RoomServer;

namespace NSL.Node.BridgeTransportExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RoomServerStartupEntry.CreateDefault().RunEntry();

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}
