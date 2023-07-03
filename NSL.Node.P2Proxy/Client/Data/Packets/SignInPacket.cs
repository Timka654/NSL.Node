using NSL.Node.P2Proxy.Client.Data;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.Node.P2Proxy.Client
{
    public partial class P2ProxyEntry
    {
        private void SignInPacketHandle(P2PNetworkClient client, InputPacketBuffer buffer)
        {
            var response = OutputPacketBuffer.Create(RoomPacketEnum.SignSessionResult);

            client.Id = buffer.ReadGuid();

            var room = this.roomMap.GetOrAdd(buffer.ReadString(), key => new Lazy<ProxyRoomInfo>(() => new ProxyRoomInfo(key)));

            var searchId = buffer.ReadNullable(() => buffer.ReadGuid());

            response.WriteBool(true);

            response.WriteBool(!searchId.HasValue || room.Value.ExistsClient(searchId.Value));

            client.Network.Send(response);
        }
    }
}
