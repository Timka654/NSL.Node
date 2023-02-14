﻿using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using SimpleGame;
using SimpleGame.Game;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NSL.Node.BridgeTransportClient.Transport.Data
{
    public class RoomInfo
    {
        private GameModel gameModel;


        private ConcurrentDictionary<Guid, TransportNetworkClient> nodes = new ConcurrentDictionary<Guid, TransportNetworkClient>();

        private Dictionary<ushort, Action<TransportNetworkClient, InputPacketBuffer>> commands = new Dictionary<ushort, Action<TransportNetworkClient, InputPacketBuffer>>();
        public Guid RoomId { get; }

        public int ConnectedNodesCount => nodes.Count;

        public IEnumerable<TransportNetworkClient> Nodes => nodes.Values;

        public DateTime CreateTime { get; } = DateTime.UtcNow;

        public RoomInfo(Guid roomId)
        {
            this.RoomId = roomId;
            //gameModel = new GameModel(2);
            commands.Add(1, (client, packet) =>
            {
                Console.WriteLine("Command Works");
            });
        }

        public void AddClient(TransportNetworkClient node)
        {
            if (nodes.TryAdd(node.Id, node))
            {
                node.Room = this;

                broadcastDelegate += node.Network.Send;
                BroadcastChangeNodeList();
            }
        }

        public bool ValidateNodeReady(TransportNetworkClient node, int totalNodeCount, IEnumerable<Guid> nodeIds)
        {
            if (!nodes.ContainsKey(node.Id))
            {
                if (node.RoomId != RoomId)
                {
                    node.Network.Disconnect();
                    return false;
                }
                //else // m.b write change room logic or not...
                //    AddClient(node);

                return false;
            }
            if (ConnectedNodesCount != nodeIds.Count() /*totalNodeCount*/)
            {
                BroadcastChangeNodeList();
#if REQUIRE_ALL_CONNECTED_NODES
                return false;
#endif
            }

            node.Ready = true;
#if REQUIRE_ALL_CONNECTED_NODES
            if (Nodes.All(x => x.Ready))
                Broadcast(CreateReadyRoomPacket())
#else
            SendTo(node, CreateReadyRoomPacket());
#endif

            return true;
        }

        private OutputPacketBuffer CreateReadyRoomPacket()
        {
            var p = OutputPacketBuffer.Create(NodeTransportPacketEnum.ReadyRoom);

            p.WriteDateTime(CreateTime);

            return p;
        }

        private void BroadcastChangeNodeList()
        {
            var buffer = OutputPacketBuffer.Create(NodeTransportPacketEnum.ChangeNodeList);

            buffer.WriteCollection(Nodes, b =>
            {
                buffer.WriteGuid(b.Id);
                buffer.WriteString16(b.Token);
                buffer.WriteString16(b.EndPoint);
            });

            Broadcast(buffer);
        }

        private Action<OutputPacketBuffer, bool> broadcastDelegate = (packet, disposeOnSend) => { };

        public void Broadcast(OutputPacketBuffer packet)
        {
            broadcastDelegate(packet, false);

            packet.Dispose();
        }

        public void SendTo(Guid nodeId, OutputPacketBuffer packet)
        {
            if (nodes.TryGetValue(nodeId, out var node))
                SendTo(node, packet);
            else
                packet.Dispose();
        }
        public void StartGame()
        {
            gameModel = new GameModel(nodes.Count);
            gameModel.StartNewTurn();
            var packet = OutputPacketBuffer.Create(NodeTransportPacketEnum.Transport);
            packet.WriteInt16((Int16)GameCommandsEnum.GameStart);

            var playersIds = nodes.Keys.ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                packet.WriteInt32(i);
                nodes[playersIds[i]].Network.Send(packet);
            }
        }
        public void Execute(TransportNetworkClient client, InputPacketBuffer packet)
        {
            var body = packet.GetBuffer();

            var to = packet.ReadGuid();

            if (commands.TryGetValue(packet.ReadUInt16(), out var command))
            {
                command(client, packet);
            }

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(NodeTransportPacketEnum.Transport);

            pbuf.WriteGuid(client.Id);

            pbuf.Write(body[0..7]);
            pbuf.Write(body[23..]);

            SendTo(to, pbuf);

        }

        public void SendTo(TransportNetworkClient node, OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            node.Network.Send(packet, disposeOnSend);
        }


    }
}