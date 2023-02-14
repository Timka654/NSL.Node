using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Logger.Interface;
using NSL.Logger;
using NSL.SocketCore.Utils.Buffer;
using NSL.BuilderExtensions.SocketCore;

using NetworkClient = NSL.Node.BridgeServer.RS.RoomServerNetworkClient;
using NetworkOptions = NSL.WebSockets.Server.WSServerOptions<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NetworkListener = NSL.WebSockets.Server.WSServerListener<NSL.Node.BridgeServer.RS.RoomServerNetworkClient>;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.ConfigurationEngine;
using System.Collections.Concurrent;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Node.BridgeServer.RS
{
    public class RoomServerEntry
    {
        protected BaseConfigurationManager Configuration => Entry.Configuration;

        public virtual int BindingPort => Configuration.GetValue("transport.server.port", 6998);

        public virtual string IdentityKey => Configuration.GetValue<string>("transport.server.identityKey", "AABBCC");

        public virtual int TransportServerCountPerRoom => Configuration.GetValue("transport.server.count.perRoom", 1);

        protected NetworkListener Listener { get; private set; }

        protected ILogger Logger { get; }

        protected BridgeServerStartupEntry Entry { get; }

        public static RoomServerEntry Create(BridgeServerStartupEntry entry, string logPrefix = "[TransportServer]")
            => new RoomServerEntry(entry, logPrefix);

        public RoomServerEntry(BridgeServerStartupEntry entry, string logPrefix = "[TransportServer]")
        {
            Entry = entry;

            if (Entry.Logger != null)
                Logger = new PrefixableLoggerProxy(Entry.Logger, logPrefix);
        }

        public RoomServerEntry Run()
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

            return this;
        }

        private void SignServerReceiveHandle(NetworkClient client, InputPacketBuffer data)
        {
            var response = data.CreateWaitBufferResponse()
                .WithPid(NodeBridgeTransportPacketEnum.SignServerResultPID);

            client.Id = data.ReadGuid();

            client.ConnectionEndPoint = data.ReadString16();

            var serverIdentityKey = data.ReadString16();


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
            else
                client.Id = Guid.NewGuid();

            while (!serverMap.TryAdd(client.Id, client))
            {
                client.Id = Guid.NewGuid();
            }

            response.WriteBool(true);

            response.WriteGuid(client.Id);

            client.Send(response);
        }

        private void SignSessionReceiveHandle(NetworkClient client, InputPacketBuffer data)
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

            packet.WriteGuid(session.RoomId);

            client.Send(packet);
        }

        internal List<CreateSignResult> CreateSignSession(string identityKey, Guid roomId)
        {
            List<CreateSignResult> result = new List<CreateSignResult>();

            var serverArray = serverMap.Values.ToArray();

            var offset = roomId.GetHashCode() % serverArray.Length;

            // single server for now - maybe change to multiple later

            var selectedServers = serverArray.Skip(offset).Take(TransportServerCountPerRoom).SingleOrDefault();


            var tsession = new TransportSession(identityKey, roomId);

            Guid newId;

            do
            {
                newId = Guid.NewGuid();
            } while (!selectedServers.SessionMap.TryAdd(newId, tsession));

            tsession.TransportIdentity = newId;

            result.Add(new CreateSignResult(selectedServers.ConnectionEndPoint, newId));

            return result;


            //if (selectedServers.Count() < TransportServerCountPerRoom)
            //{
            //    offset = 0;

            //    serverArray = selectedServers.Concat(serverArray.Skip(offset).Take(TransportServerCountPerRoom - selectedServers.Count())).ToArray();
            //}


            //foreach (var item in serverArray)
            //{
            //    Guid newid = default;

            //    var tsession = new TransportSession() { IdentityKey = identityKey };

            //    do
            //    {
            //        newid = Guid.NewGuid();
            //    } while (!item.SessionMap.TryAdd(newid, tsession));

            //    result.Add(new CreateSignResult(item.ConnectionEndPoint, newid));
            //}

            //return result;
        }

        private ConcurrentDictionary<Guid, NetworkClient> serverMap = new ConcurrentDictionary<Guid, NetworkClient>();

        public record CreateSignResult(string endPoint, Guid id);
    }
}
