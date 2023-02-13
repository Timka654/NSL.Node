using NSL.Node.BridgeServer;

namespace NSL.Node.BridgeServerExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BridgeServerEntry.CreateDefault().RunEntry();

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}