namespace NSL.Node.BridgeServerExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BridgeServerStartupEntry.CreateDefault().RunEntry();

            Console.WriteLine(">>> Success initialized");

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}