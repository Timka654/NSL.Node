using System.Security.Cryptography.X509Certificates;

namespace SimpleGame
{
    public class Tile
    {
        public Terrain Terrain { get; set; }
        public Player Controller { get; set; }
        public Player CurrentPlayer { get; set; }
        public (int Y, int X) Coordinates { get; set; }
    }

    public enum Terrain
    {
        Water,
        Ground,
        FOW
    }





}