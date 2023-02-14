using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeServer.Shared.Enums
{
    public enum NodeBridgeTransportPacketEnum : ushort
    {
        SignServerPID = 1,
        SignServerResultPID = SignServerPID,
        SignSessionPID,
        SignSessionResultPID = SignSessionPID
    }
}
