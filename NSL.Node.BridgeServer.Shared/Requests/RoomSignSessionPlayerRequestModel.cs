using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class RoomSignSessionPlayerRequestModel
    {
        public Guid SessionId { get; set; }

        public Guid PlayerId { get; set; }
    }
}
