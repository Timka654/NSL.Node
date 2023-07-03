using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.Shared.Response
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class CreateRoomSessionResponseModel
    {
        public bool Result { get; set; }

        public List<RoomServerPointInfo> ConnectionPoints { get; set; }
    }
}
