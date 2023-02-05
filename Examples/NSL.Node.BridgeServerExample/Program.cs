namespace NSL.Node.BridgeServerExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new Bridge().Run();

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}