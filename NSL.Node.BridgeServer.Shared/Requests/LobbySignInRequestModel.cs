using NSL.Generators.BinaryTypeIOGenerator.Attributes;

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
