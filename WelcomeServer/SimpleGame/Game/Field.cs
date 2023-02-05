namespace SimpleGame
{
    public class Field
    {
        public Tile[,] Tiles;


        public Field(int width = 100, int height = 100)
        {
            Tiles = GenerateTiles(height, width);

            foreach (var t in Tiles)
            {
                t.SetNeighbours(this);
            }
        }

        internal void SetPlayers(List<Player> players)
        {
            if (players.Count == 2)
            {
                players[0].Territory.Add(Tiles[0, 0]);
                players[1].Territory.Add(Tiles[Tiles.GetLength(0) - 1, Tiles.GetLength(1) - 1]);
            }
            else if (players.Count == 4)
            {

                players[0].Territory.Add(Tiles[0, 0]);
                players[1].Territory.Add(Tiles[Tiles.GetLength(0) - 1, 0]);
                players[2].Territory.Add(Tiles[0, Tiles.GetLength(1) - 1]);
                players[3].Territory.Add(Tiles[Tiles.GetLength(0) - 1, Tiles.GetLength(1) - 1]);
            }

            for (int i = 0; i < players.Count; i++)
            {
                players[i].Territory[0].ChangeController(players[i]);
                players[i].Territory[0].Power = 2;
            }
        }

        private Tile[,] GenerateTiles(int height, int width)
        {
            var res = new Tile[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    res[i, j] = new Tile();
                    res[i, j].SetCoordinates(i, j);
                }
            }
            return res;
        }
    }
}