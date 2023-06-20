using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class CreateRoomSessionRequestModel
    {
        public Guid RoomId { get; set; }

        public string Location { get; set; }

        public List<Guid> InitialPlayers { get; set; }

        public int NeedPointCount { get; set; }
    }
}
