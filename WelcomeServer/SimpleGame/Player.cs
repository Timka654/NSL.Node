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
        public bool[,] MapMask { get; set; }
        public Tile CurrentTile { get; set; }
        public int Money { get; set; }
        public int BaseMovement { get; set; }
        public int MovementPointsLeft { get; set; }

        internal GameModel GetMaskedInfo(GameModel gameModel)
        {
            var ownModel = gameModel;

            ownModel.Field = new Field();
            ownModel.Field.CopyTilesFrom(ownModel.Field, gameModel.Field);

            ownModel = ownModel.SetMask(Team, MapMask, Money, MovementPointsLeft);

            return ownModel;
            // copy gm value
        }

        internal void InitMask(int h, int w)
        {
            MapMask = new bool[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var yReal = h - 1 - y;
                    if (Math.Abs(yReal - CurrentTile.Coordinates.Y) + Math.Abs(x - CurrentTile.Coordinates.X) < 8)
                    {
                        MapMask[yReal, x] = true;
                    }
                }
            }

            MovementPointsLeft = BaseMovement;
            Money = 0;
        }

    }
}
