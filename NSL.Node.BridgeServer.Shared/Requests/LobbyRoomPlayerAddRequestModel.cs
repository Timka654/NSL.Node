using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class LobbyRoomPlayerAddRequestModel
    {
        public Guid RoomId { get; set; }

        public Guid PlayerId { get; set; }
    }
}
