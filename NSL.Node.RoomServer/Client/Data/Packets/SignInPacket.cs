using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Client
{
    public partial class ClientServerBaseEntry
    {
        private async Task SignInPacketHandle(TransportNetworkClient client, InputPacketBuffer buffer)
        {
            var response = OutputPacketBuffer.Create(RoomPacketEnum.SignSessionResult);

            var sessionId = buffer.ReadGuid();

            var roomId = buffer.ReadGuid();

            var token = buffer.ReadString();

            client.EndPoint = buffer.ReadString();

            bool success = false;

            var roomInfo = await TryLoadRoomAsync(roomId, sessionId);

            if (roomInfo != null)
            {
                if (Guid.TryParse(token.Split(':').First(), out var nodeId))
                {
                    var validatePlayer = await Entry.ValidateSessionPlayer(new BridgeServer.Shared.Requests.RoomSignSessionPlayerRequestModel()
                    {
                        SessionId = sessionId,
                        PlayerId = nodeId
                    });

                    if (!validatePlayer.ExistsSession)
                    {
                        if (roomMap.TryRemove(sessionId, out var expiredSession))
                        {
                            expiredSession.Value.Dispose();
                        }
                    }

                    if (validatePlayer.ExistsPlayer)
                    {
                        client.Room = roomInfo;
                        client.Id = client.NodeId = nodeId;
                        client.Token = token;

                        success = client.Room.AddClient(client);;
                    }
                }
            }

            response.WriteBool(success);

            if (success)
                response.WriteGuid(client.NodeId);

            client.Network.Send(response);
        }
    }
}
