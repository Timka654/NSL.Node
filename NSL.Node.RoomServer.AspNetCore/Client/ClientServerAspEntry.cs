using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.Node.RoomServer.Client;
using System.Threading.Tasks;
using System;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.RoomServer.Client.Data;
using NSL.BuilderExtensions.WebSocketsServer.AspNet;
using NSL.WebSockets.Server;

namespace NSL.Node.RoomServer.AspNetCore.Client
{
    public class ClientServerAspEntry : ClientServerBaseEntry
    {
        private readonly IEndpointRouteBuilder builder;
        private readonly string pattern;
        private readonly Func<HttpContext, Task<bool>> requestHandle;
        private readonly Action<IEndpointConventionBuilder> actionConventionBuilder;

        public ClientServerAspEntry(
            NodeRoomServerEntry entry,
            IEndpointRouteBuilder builder,
            string pattern,
            Func<HttpContext, Task<bool>> requestHandle = null,
            Action<IEndpointConventionBuilder> actionConventionBuilder = null,
            string logPrefix = null) : base(entry, logPrefix)
        {
            this.builder = builder;
            this.pattern = pattern;
            this.requestHandle = requestHandle;
            this.actionConventionBuilder = actionConventionBuilder;
        }

        public override void Run()
        {
            if (Listener == null)
            {
                var server = Fill(WebSocketsServerEndPointBuilder.Create()
                    .WithClientProcessor<TransportNetworkClient>()
                    .AspWithOptions<TransportNetworkClient, WSServerOptions<TransportNetworkClient>>())
                    .BuildWithoutRoute();

                Listener = server;

                var acceptDelegate = server.GetAcceptDelegate();

                var convBuilder = builder.Map(pattern, async context =>
                {
                    if (requestHandle != null)
                        if (!await requestHandle(context))
                            return;

                    await acceptDelegate(context);
                });

                if (actionConventionBuilder != null)
                    actionConventionBuilder(convBuilder);
            }
        }
    }
}
