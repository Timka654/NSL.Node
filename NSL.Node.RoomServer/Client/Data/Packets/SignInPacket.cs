using NSL.Node.BridgeServer.Shared;
using NSL.Node.Core.Models.Requests;
using NSL.Node.Core.Models.Response;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core.Enums;
using NSL.SocketCore.Extensions.Buffer;
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
            var response = buffer.CreateResponse();

            var request = RoomNodeSignInRequestModel.ReadFullFrom(buffer);

            var result = new RoomNodeSignInResponseModel();

            client.EndPoint = request.ConnectionEndPoint;

            var roomInfo = await TryLoadRoomAsync(request.RoomId, request.SessionId);

            if (roomInfo != null)
            {
                var splitedToken = request.Token.Split(':');

                if (Guid.TryParse(splitedToken.First(), out var nodeId))
                {
                    var validatePlayer = await Entry.ValidateSessionPlayer(new BridgeServer.Shared.Requests.RoomSignSessionPlayerRequestModel()
                    {
                        SessionId = request.SessionId,
                        PlayerId = nodeId
                    });

                    if (!validatePlayer.ExistsSession)
                    {
                        if (roomMap.TryRemove(request.SessionId, out var expiredSession))
                        {
                            client.Network?.Options.HelperLogger?.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $" - [SignIn] Remove session {request.SessionId} by no exists information on Bridge");
                            expiredSession.Value.Dispose();
                        }
                    }

                    if (validatePlayer.ExistsPlayer)
                    {
                        client.Room = roomInfo;
                        client.Id = client.NodeId = nodeId;
                        client.Token = string.Join(':', splitedToken.Skip(1).ToArray());

                        result.Success = client.Room.AddClient(client);

                        if (result.Success)
                        {
                            result.NodeId = client.NodeId;
                            result.Options = roomInfo.GetClientOptions();
                            var session = sessionManager.CreateSession(client, client.NodeId.ToString());
                            result.SessionInfo = session;
                        }
                    }
                }
            }

            result.WriteFullTo(response);

            client.Network.Send(response);
        }
    }
}
