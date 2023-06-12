using NSL.Node.BridgeServer.Shared;
using System;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient.Models
{
    public class BridgeLobbyNetworkHandlesConfigurationModel
    {
        public ValidateSessionDelegate ValidateSessionHandle { set; internal get; } = (room, sessionIdentity) => Task.FromResult(false);

        public RoomDataMessageDelegate RoomFinishHandle { set; internal get; } = (room, sessionIdentity) => Task.CompletedTask;

        public RoomDataMessageDelegate RoomMessageHandle { set; internal get; } = (room, sessionIdentity) => Task.CompletedTask;
    }

    public delegate Task<bool> ValidateSessionDelegate(Guid roomId, string sessionIdentity);

    public delegate Task RoomDataMessageDelegate(Guid roomId, byte[] data);
}
