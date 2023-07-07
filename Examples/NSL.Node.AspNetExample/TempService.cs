using NSL.Node.BridgeLobbyClient.Models;

namespace NSL.Node.AspNetExample
{
    public class TempService
    {
        internal void ConfigureNodeHandles(BridgeLobbyNetworkHandlesConfigurationModel handles)
        {
            handles.RoomMessageHandle = BridgeRoomMessageAsync;
            handles.RoomFinishHandle = BridgeRoomFinishAsync;
        }

        private Task BridgeRoomFinishAsync(Guid roomId, byte[] data)
        {
            return Task.CompletedTask;
        }

        private Task BridgeRoomMessageAsync(Guid roomId, byte[] data)
        {
            return Task.CompletedTask;
        }
    }
}
