
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Shared
{
    public class GameInfo
    {
        public IRoomInfo RoomInfo { get; }

        public GameInfo(IRoomInfo roomInfo)
        {
            RoomInfo = roomInfo;
        }
    }
}
