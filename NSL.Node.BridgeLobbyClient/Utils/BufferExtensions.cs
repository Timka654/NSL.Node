using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeLobbyClient.Utils
{
    public static class BufferExtensions
    {
        public static OutputPacketBuffer CreateResponse(this InputPacketBuffer input)
        {
            var response = input.CreateResponse();
            response.PacketId = 1;
            return response;
        }
    }
}
