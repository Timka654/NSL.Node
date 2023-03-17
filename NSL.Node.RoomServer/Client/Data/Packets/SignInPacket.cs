using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerEntry
    {
        private async Task SignInPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var response = OutputPacketBuffer.Create(RoomPacketEnum.SignSessionResult);

            client.Token = buffer.ReadString16();

            client.Id = buffer.ReadGuid();

            client.EndPoint = buffer.ReadString16();

            var result = await bridgeNetwork.TryAuthorize(client);

            response.WriteBool(result);

            if (result)
            {
                client.NodeId = Guid.Parse(client.Token.Split(':').First());

                response.WriteGuid(client.NodeId);

                client.Room = roomMap.GetOrAdd((client.LobbyServerIdentity, client.RoomId), id =>
                {
                    var room = new RoomInfo(Entry,id.roomId, id.lobbyServerIdentity);

                    return room;
                });

                client.Room.AddClient(client);

                LoadRoomStartupInfo(client.Room);
            }
            else
            {
                client.Token = default;
                client.Id = default;
            }

            client.Network.Send(response);
        }

        private AutoResetEvent loadStartupInfoLocker = new AutoResetEvent(true);

        private async void LoadRoomStartupInfo(RoomInfo room)
        {
            loadStartupInfoLocker.WaitOne();

            try
            {
                if (room.StartupInfo != null)
                {
                    loadStartupInfoLocker.Set();
                    return;
                }

                var startupInfoTask = await bridgeNetwork.GetRoomStartupInfo(room);

                if (startupInfoTask.Item1)
                    room.SetStartupInfo(startupInfoTask.Item2);
                else
                    Logger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"Cannot receive startupInfo for {room.LobbyServerIdentity} - {room.RoomId}");
            }
            finally
            {
                loadStartupInfoLocker.Set();
            }
        }
    }
}
