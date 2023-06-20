using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class LobbySignInRequestModel
    {
        public string Identity { get; set; }

        public string IdentityKey { get; set; }

        public bool IsRecovery { get; set; }
    }
}
