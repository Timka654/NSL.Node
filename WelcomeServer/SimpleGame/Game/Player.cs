using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGame
{
    public class Player
    {
        public int Team { get; set; }
        public Tile CurrentTile { get; set; }
        public List<Tile> Territory { get; set; }
        public TurnPhase TurnPhase { get; set; }
        public int Power { get; set; }
        public List<Tile> ActiveTiles => Territory.Where(tile => tile.Neigbours.Any(x => !Territory.Contains(x)) && tile.Power > 1).ToList();
    }
    public enum TurnPhase
    {
        ExpandPhase,
        FinishedExpandPhase,
        EconomicsPhase,
        FinishedTurn
    }
}
