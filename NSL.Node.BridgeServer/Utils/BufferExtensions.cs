using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeServer.Utils
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
