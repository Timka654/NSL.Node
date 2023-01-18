namespace SimpleGame
{
    public class Field
    {
        public Tile[,] Tiles;


        public Field(int width = 100, int height = 100)
        {
            Tiles = GenerateTiles(height, width);
        }

        public void CopyTilesFrom(Field target, Field origin)
        {
            target.Tiles = new Tile[origin.Tiles.GetLength(0), origin.Tiles.GetLength(1)];
            for (int i = 0; i < Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < Tiles.GetLength(1); j++)
                {
                    target.Tiles[i, j] = new Tile();
                    target.Tiles[i, j].Coordinates = origin.Tiles[i, j].Coordinates;
                    target.Tiles[i, j].Terrain = origin.Tiles[i, j].Terrain;
                    target.Tiles[i, j].Controller = origin.Tiles[i, j].Controller;
                    target.Tiles[i, j].CurrentPlayer = origin.Tiles[i, j].CurrentPlayer;
                }
            }
        }
        internal void SetPlayers(Player[] players)
        {
            for (int i = 0; i < players.Length; i++)
            {
                var rand = new Random();
                int x, y;

                do
                {
                    x = rand.Next(Tiles.GetLength(0));
                    do
                    {
                        y = rand.Next(Tiles.GetLength(0));
                    } while (x == y);
                }
                while (Tiles[x, y].Terrain == Terrain.Water);

                players[i].CurrentTile = Tiles[x, y];
                players[i].CurrentTile.Controller = players[i];
                players[i].CurrentTile.CurrentPlayer = players[i];
            }
        }

        private Tile[,] GenerateTiles(int height, int width)
        {
            var res = new Tile[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Tiles[height - i - 1, j] = new Tile();
                    Tiles[height - i - 1, j].Coordinates = (height - i - 1, j);
                    if (j > width / 3 && j < width * 2 / 3)
                    {
                        if (Math.Abs(i - height / 2) < 10)
                        {
                            Tiles[height - i - 1, j].Terrain = Terrain.Ground;
                        }
                        else
                        {
                            Tiles[height - 1 - i, j].Terrain = Terrain.Water;
                        }
                    }
                    else
                    {
                        Tiles[height - i - 1, j].Terrain = Terrain.Ground;
                    }
                }
            }
            return res;
        }
    }
}