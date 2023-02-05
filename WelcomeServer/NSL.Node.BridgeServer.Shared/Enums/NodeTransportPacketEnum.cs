using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeTransportPacketEnum
    {
        SignSession = 1,
        SignSessionResult = SignSession,
        ChangeNodeList,
        Transport,
        Broadcast,
        ReadyNodePID,
        ReadyNodeResultPID = ReadyNodePID,
        ReadyRoom
    }
}
