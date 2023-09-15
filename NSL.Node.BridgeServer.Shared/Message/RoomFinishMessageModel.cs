using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.Node.BridgeServer.Shared.Message
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class RoomFinishMessageModel : RoomMessageModel
    {
    }
}
