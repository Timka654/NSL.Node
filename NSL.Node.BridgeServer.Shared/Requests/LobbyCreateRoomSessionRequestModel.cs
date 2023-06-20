using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class LobbyCreateRoomSessionRequestModel
    {
        public Guid RoomId { get; set; }

        public string Location { get; set; }

        public Guid? SpecialServer { get; set; }

        public List<Guid> InitialPlayers { get; set; }

        public int NeedPointCount { get; set; } = 1;

        public Dictionary<string, string> StartupOptions { get; set; }
    }
}
