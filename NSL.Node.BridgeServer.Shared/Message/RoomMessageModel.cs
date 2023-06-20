using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeServer.Shared.Message
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class RoomMessageModel
    {
        public Guid SessionId { get; set; }

        public byte[] Data { get; set; }
    }
}
