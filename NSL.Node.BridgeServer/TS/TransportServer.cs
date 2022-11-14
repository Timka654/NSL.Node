using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.TS.TransportServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.TS.TransportServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.TS.TransportServerNetworkClient>;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.ConfigurationEngine;
using System.Collections.Concurrent;
using NSL.SocketCore.Extensions.Buffer;
using System.Collections.Generic;
using NSL.Node.BridgeServer.CS;

namespace NSL.Node.BridgeServer.TS
{
    internal class TransportServer
    {
        private static BaseConfigurationManager Configuration => Program.Configuration;

        public static int BindingPort => Configuration.GetValue("transport.server.port", 6998);

        public static string IdentityKey => Configuration.GetValue("transport.server.identityKey", "AABBCC");

        public static NetworkListener Listener { get; private set; }

        public static ILogger Logger { get; } = new PrefixableLoggerProxy(Program.Logger, "[TransportServer]");

        public static void Run()
        {
            Listener = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<NetworkClient>()
                .WithOptions<NetworkOptions>()
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);

                    builder.AddDefaultEventHandlers<WebSocketsServerEndPointBuilder<NetworkClient, NetworkOptions>, NetworkClient>(null, DefaultEventHandlersEnum.All & ~DefaultEventHandlersEnum.HasSendStackTrace);

                    builder.AddPacketHandle(NodeBridgeTransportPacketEnum.SignServerPID, SignServerReceiveHandle);
                    builder.AddPacketHandle(NodeBridgeTransportPacketEnum.SignSessionPID, SignSessionReceiveHandle);
                })
                .WithBindingPoint($"http://*:{BindingPort}/")
                .Build();

            Listener.Start();
        }

        private static void SignServerReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeTransportPacketEnum.SignServerResultPID);

            client.Id = data.ReadGuid();

            var serverIdentityKey = data.ReadString16();

            //var endPoint = data.ReadString16();

            if (!IdentityKey.Equals(serverIdentityKey))
            {
                response.WriteBool(false);

                client.Send(response);

                return;
            }

            if (!Guid.Empty.Equals(client.Id))
            {
                if (serverMap.TryGetValue(client.Id, out var server))
                {
                    if (server.GetState())
                    {
                        response.WriteBool(false);

                        client.Send(response);

                        return;
                    }

                    serverMap[client.Id] = client;

                    server.ChangeOwner(client);

                    response.WriteBool(true);

                    response.WriteGuid(client.Id);

                    client.Send(response);

                    return;
                }
            }

            while (!serverMap.TryAdd(client.Id, client))
            {
                client.Id = Guid.NewGuid();
            }

            response.WriteBool(true);

            response.WriteGuid(client.Id);

            client.Send(response);
        }

        private static void SignSessionReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var packet = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeTransportPacketEnum.SignSessionResultPID);

            var identityKey = data.ReadString16();
            var id = data.ReadGuid();

            if (!client.SessionMap.TryGetValue(id, out var session) || !session.IdentityKey.Equals(identityKey))
            {
                packet.WriteBool(false);

                client.Send(packet);

                return;
            }


            packet.WriteBool(true);

            client.Send(packet);
        }

        internal static List<CreateSignResult> CreateSignSession(string identityKey)
        {
            List<CreateSignResult> result = new List<CreateSignResult>();

            foreach (var item in serverMap.Values.ToArray())
            {
                Guid newid = default;

                var tsession = new TransportSession() { IdentityKey = identityKey };

                do
                {
                    newid = Guid.NewGuid();
                } while (!item.SessionMap.TryAdd(newid, tsession));

                result.Add(new CreateSignResult(item.PublicIPAddr, item.PublicPort, newid));
            }

            return result;
        }

        private static ConcurrentDictionary<Guid, NetworkClient> serverMap = new ConcurrentDictionary<Guid, NetworkClient>();

        public record CreateSignResult(string ipAddr, int port, Guid id);
    }
}
