using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class RoomSignSessionRequestModel
    {
        public Guid SessionIdentity { get; set; }

        public Guid RoomIdentity { get; set; }
    }
}
