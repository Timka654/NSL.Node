using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.BridgeServer.Utils
{
    public static class BufferExtensions
    {
        public static OutputPacketBuffer CreateResponse(this InputPacketBuffer input)
        {
            var response = input.CreateWaitBufferResponse();

            response.PacketId = 1; // in all packet type enum must have Response = 1

            return response;
        }
    }
}
