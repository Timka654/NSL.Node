using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Shared;
using NSL.Node.RoomServer.Shared.Client.Core;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Client.Data
{
    public class RoomInfo : IRoomInfo, IDisposable
    {
        private AutoResetEvent ar = new AutoResetEvent(false);

        private ConcurrentDictionary<Guid, TransportNetworkClient> nodes = new ConcurrentDictionary<Guid, TransportNetworkClient>();

        public IEnumerable<PlayerInfo> GetNodes() { return nodes.Values.Select(x=>x.Player).ToArray(); }

        private Dictionary<ushort,
            ReciveHandleDelegate> handles = new Dictionary<ushort, ReciveHandleDelegate>();

        private Action<OutputPacketBuffer, bool> broadcastDelegate = (packet, disposeOnSend) => { };

        private SGameInfo Game;

        public RoomServerStartupEntry Entry { get; }

        public Guid RoomId { get; }

        public string LobbyServerIdentity { get; }

        public int ConnectedNodesCount => nodes.Count;

        public IEnumerable<TransportNetworkClient> Nodes => nodes.Values;

        public DateTime CreateTime { get; } = DateTime.UtcNow;

        public NodeRoomStartupInfo StartupInfo { get; private set; }

        public int RoomPlayerCount { get; private set; }

        public bool RoomWaitAllReady { get; private set; }

        public int StartupTimeout { get; private set; }

        public bool ShutdownOnMissedReady { get; private set; }

        public event Action<PlayerInfo> OnNodeConnect = player => { };

        public event Action OnRoomReady = () => { };

        public RoomInfo(RoomServerStartupEntry entry, Guid roomId, string lobbyServerIdentity)
        {
            Entry = entry;
            RoomId = roomId;
            LobbyServerIdentity = lobbyServerIdentity;
            Game = new SGameInfo(this);
        }

        public bool AddClient(TransportNetworkClient node)
        {
            if (nodes.TryAdd(node.Id, node))
            {
                node.Player = new PlayerInfo() { Network = node, Id = node.Id };

                if (node.Network != null)
                    broadcastDelegate += node.Network.Send;

                BroadcastChangeNodeList();

                return true;
            }

            return false;
        }

        internal void SetStartupInfo(NodeRoomStartupInfo startupInfo)
        {
            StartupInfo = startupInfo;

            RoomWaitAllReady = startupInfo.GetRoomWaitReady();

            RoomPlayerCount = startupInfo.GetRoomPlayerCount();

            StartupTimeout = startupInfo.GetRoomStartupTimeout();

            ShutdownOnMissedReady = startupInfo.GetRoomShutdownOnMissed();

            if (ShutdownOnMissedReady)
                RunDestroyOnMissedTimer();

            ar.Set();
        }

        private async void RunDestroyOnMissedTimer()
        {
            await Task.Delay(StartupTimeout);

            if (ConnectedNodesCount == RoomPlayerCount)
                return;

            Dispose();
        }

        public bool ValidateNodeReady(TransportNetworkClient node, int totalNodeCount, IEnumerable<Guid> nodeIds)
        {
            if (!ar.WaitOne(5000))
            {
                throw new Exception();
            }
            //ar.WaitOne();
            if (!nodes.ContainsKey(node.Id))
            {
                if (node.RoomId != RoomId)
                {
                    node.Network.Disconnect();

                    ar.Set();

                    return false;
                }

                ar.Set();

                return false;
            }
            if (ConnectedNodesCount != nodeIds.Count())
            {
                BroadcastChangeNodeList();

                ar.Set();

                return false;
            }

            if (ConnectedNodesCount != RoomPlayerCount && RoomWaitAllReady)
            {
                ar.Set();
                return false;
            }

            node.Ready = true;

            OnNodeConnect(node.Player);

            if (RoomWaitAllReady)
            {
                if (Nodes.All(x => x.Ready))
                {
                    Broadcast(CreateReadyRoomPacket());
                    OnRoomReady();
                }
            }
            else
                SendTo(node, CreateReadyRoomPacket());

            ar.Set();

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

        public void SendToRoomServer(OutputPacketBuffer packet)
        {
        }

        public void Dispose()
        {
            Entry.BridgeClient.FinishRoom(this, null);
        }

        public bool Broadcast(Action<OutputPacketBuffer> builder, ushort code)
        {
            return Broadcast(packet =>
            {
                packet.WriteUInt16(code);
                builder(packet);
            });
        }

        public bool Broadcast(Action<OutputPacketBuffer> builder)
        {
            var packet = OutputPacketBuffer.Create(RoomPacketEnum.Execute);

            builder(packet);

            Broadcast(packet);

            return true;
        }
    }
}
