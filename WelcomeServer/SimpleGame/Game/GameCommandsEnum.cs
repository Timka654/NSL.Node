using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGame.Game
{
    public enum GameCommandsEnum
    {
        GameStart=1,
        GameFinish,
        PlayerExpand,
        PlayerGrowTile,
        PlayerFinishedExpand,
        PlayerFinishedTurn,
        StartNewTurn,
        StartGrowPhase
    }
}
