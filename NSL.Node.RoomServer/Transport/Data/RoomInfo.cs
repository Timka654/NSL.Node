﻿using NSL.Node.BridgeServer.Shared.Enums;
using NSL.Node.RoomServer.Shared;
using NSL.Node.RoomServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Transport.Data
{
    public class RoomInfo : IRoomInfo
    {
        private ConcurrentDictionary<Guid, TransportNetworkClient> nodes = new ConcurrentDictionary<Guid, TransportNetworkClient>();

        private Dictionary<ushort,
            ReciveHandleDelegate> handles = new Dictionary<ushort, ReciveHandleDelegate>();

        public Guid RoomId { get; }

        public int ConnectedNodesCount => nodes.Count;

        public IEnumerable<TransportNetworkClient> Nodes => nodes.Values;

        public DateTime CreateTime { get; } = DateTime.UtcNow;

        private GameInfo Game;

        public RoomInfo(Guid roomId) { this.RoomId = roomId; Game = new GameInfo(this); }

        public bool AddClient(TransportNetworkClient node)
        {
            if (nodes.TryAdd(node.Id, node))
            {
                if (node.Network != null)
                    broadcastDelegate += node.Network.Send;

                node.Player = new PlayerInfo() { Network = node, Id = node.Id };

                BroadcastChangeNodeList();

                return true;
            }

            return false;
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

        protected OutputPacketBuffer CreateReadyRoomPacket()
        {
            var p = OutputPacketBuffer.Create(RoomPacketEnum.ReadyRoom);

            p.WriteDateTime(CreateTime);

            return p;
        }

        private void BroadcastChangeNodeList()
        {
            var buffer = OutputPacketBuffer.Create(RoomPacketEnum.ChangeNodeList);

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

        public void SendTo(TransportNetworkClient node, OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            node.Network?.Send(packet, disposeOnSend);
        }

        public void Execute(TransportNetworkClient client, InputPacketBuffer packet)
        {
            if (handles.TryGetValue(packet.ReadUInt16(), out var command))
            {
                command(client.Player, packet);
            }
        }

        public void Transport(TransportNetworkClient client, InputPacketBuffer packet)
        {
            var body = packet.GetBuffer();

            var to = packet.ReadGuid();

            OutputPacketBuffer pbuf = OutputPacketBuffer.Create(RoomPacketEnum.Transport);

            pbuf.WriteGuid(client.Id);

            pbuf.Write(body[0..7]);
            pbuf.Write(body[23..]);

            SendTo(to, pbuf);
        }

        public void SendTo(PlayerInfo player, OutputPacketBuffer packet, bool disposeOnSend = true)
        {
            SendTo(player.Network as TransportNetworkClient, packet, disposeOnSend);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterHandle(ushort code, ReciveHandleDelegate handle)
        {
            if (!handles.TryAdd(code, handle))
                throw new Exception($"code {code} already contains in {nameof(handles)}");
        }


        public PlayerInfo GetPlayer(Guid id)
        {
            if (nodes.TryGetValue(id, out var node))
                return node.Player;

            return default;
        }

        public void Execute(ushort command, Action<OutputPacketBuffer> build)
        {
        }

        public void SendToGameServer(OutputPacketBuffer packet)
        {
        }
    }
}
