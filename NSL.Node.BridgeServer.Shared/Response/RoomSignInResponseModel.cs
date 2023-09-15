using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Node.BridgeServer.Shared.Response
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class RoomSignInResponseModel
    {
        public bool Result { get; set; }

        public Guid ServerIdentity { get; set; }
    }
}
