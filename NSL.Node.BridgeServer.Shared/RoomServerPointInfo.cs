using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeServer.Shared
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class RoomServerPointInfo
    {
        public string Endpoint { get; set; }

        public Guid RoomId { get; set; }
    }
}
