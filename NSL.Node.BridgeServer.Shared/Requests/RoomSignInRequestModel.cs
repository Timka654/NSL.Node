using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class RoomSignInRequestModel
    {
        public Guid Identity { get; set; }

        public string IdentityKey { get; set; }

        public string ConnectionEndPoint { get; set; }
    }
}
