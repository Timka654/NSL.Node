using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.RoomServer;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Transport;
using NSL.Node.RoomServer.Transport.Data;
using NSL.UDP.Client.Info;
using NSL.WebSockets.Client;
using NSL.WebSockets.Server;
using STUN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RoomServerEntry.CreateDefault().RunEntry();

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}
