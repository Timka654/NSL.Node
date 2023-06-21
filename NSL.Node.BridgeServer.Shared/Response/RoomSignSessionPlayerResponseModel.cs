using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeServer.Shared.Response
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class RoomSignSessionPlayerResponseModel
    {
        public bool ExistsSession { get; set; }

        public bool ExistsPlayer { get; set; }
    }
}
