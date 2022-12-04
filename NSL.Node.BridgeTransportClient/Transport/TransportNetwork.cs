using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.BridgeTransportClient.Bridge;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using NSL.WebSockets.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Transport
{
    public class TransportNetwork
    {
        public string BindingAddress { get; }

        protected WSServerListener<TransportNetworkClient> network { get; private set; }

        public TransportNetwork(
            BridgeTransportNetwork bridgeNetwork,
            string bindingAddress,
            Action<WebSocketsServerEndPointBuilder<TransportNetworkClient, WSServerOptions<TransportNetworkClient>>> onBuild = null)
        {
            this.bridgeNetwork = bridgeNetwork;
            this.BindingAddress = bindingAddress;

            network = WebSocketsServerEndPointBuilder.Create()
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSServerOptions<TransportNetworkClient>>()
                .WithBindingPoint(bindingAddress)
                .WithCode(builder =>
                {
                    builder.AddPacketHandle(
                        BridgeServer.Shared.Enums.NodeTransportPacketEnum.SignSession,
                        SignInPacketHandle);
                    builder.AddPacketHandle(
                        BridgeServer.Shared.Enums.NodeTransportPacketEnum.Transport, TransportHandle);
                    builder.AddPacketHandle(
                        BridgeServer.Shared.Enums.NodeTransportPacketEnum.Broadcast, BroadcastHandle);

                    if (onBuild != null)
                        onBuild(builder);
                })
                .Build();

            network.Start();
        }

        private async void SignInPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var response = buffer.CreateWaitBufferResponse()
                 .WithPid(NodeTransportPacketEnum.SignSessionResult);

            client.Token = buffer.ReadString16();

            client.Id = buffer.ReadGuid();

            client.EndPoint = buffer.ReadString16();

            var result = await bridgeNetwork.TryAuthorize(client);

            response.WriteBool(result);

            if (result)
            {
                client.Room = roomMap.GetOrAdd(client.RoomId, id => new RoomInfo(id));

                client.Room.AddClient(client);
            }
            else
            {
                client.Token = default;
                client.Id = default;
            }
            client.Network.Send(response);
        }

        private void BroadcastHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var body = buffer.GetBuffer();

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(NodeTransportPacketEnum.Transport);

            pbuf.WriteGuid(client.Id);

            pbuf.Write(body);

            client.Room.Broadcast(pbuf);
        }

        private void TransportHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var body = buffer.GetBuffer();

            var to = buffer.ReadGuid();

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(NodeTransportPacketEnum.Transport);

            pbuf.WriteGuid(client.Id);

            pbuf.Write(body[0..7]);
            pbuf.Write(body[24..]);

            client.Room.SendTo(to, pbuf);
        }


        private readonly BridgeTransportNetwork bridgeNetwork;

        private ConcurrentDictionary<Guid, RoomInfo> roomMap = new ConcurrentDictionary<Guid, RoomInfo>();
    }

    public class RoomInfo
    {
        private ConcurrentDictionary<Guid, TransportNetworkClient> players = new ConcurrentDictionary<Guid, TransportNetworkClient>();

        public Guid RoomId { get; }

        public RoomInfo(Guid roomId)
        {
            this.RoomId = roomId;
        }

        public void AddClient(TransportNetworkClient player)
        {
            if (players.TryAdd(player.Id, player))
            {
                player.Room = this;

                broadcastDelegate = (Action<OutputPacketBuffer>)Action.Combine(broadcastDelegate, player.Send);
            }
        }

        private Action<OutputPacketBuffer> broadcastDelegate = (packet) => { };

        public void Broadcast(OutputPacketBuffer packet)
        {
            broadcastDelegate(packet);
        }

        public void SendTo(Guid playerId, OutputPacketBuffer packet)
        {
            if (players.TryGetValue(playerId, out var player))
                SendTo(player, packet);
        }

        public void SendTo(TransportNetworkClient player, OutputPacketBuffer packet)
        {
            player.Send(packet);
        }


    }
}
