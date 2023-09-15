using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class LobbyRoomPlayerRemoveRequestModel : LobbyRoomPlayerAddRequestModel
    {
    }
}
