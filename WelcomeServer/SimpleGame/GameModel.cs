using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGame
{
    public struct GameModel
    {
        public Field Field { get; set; }
        public Player[] _players;

        private int[] money;
        
        
        private readonly int h = 100;
        private readonly int w = 100;
        private bool[][,] informMasks;
        public GameModel(int playersCount)
        {
            _players = new Player[playersCount];
            informMasks = new bool[playersCount][,];
            for (int i = 0; i < _players.Length; i++)
            {
                var player = _players[i];
                player = new Player();
                player.Team = i;
                informMasks[i] = new bool[h, w];
                player.MapMask = informMasks[i];
            }

            Field = new Field(h, w);

            Field.SetPlayers(_players);
            for (int i = 0; i < _players.Length; i++)
            {
                _players[i].Team = i;
                _players[i].BaseMovement = 5;
                _players[i].InitMask(h, w);
            }
        }

        public GameModel[] GetMaskedInfo()
        {
            var gameModelsForPlayers = new GameModel[_players.Length];
            for (int i = 0; i < gameModelsForPlayers.Length; i++)
            {
                gameModelsForPlayers[i] = _players[i].GetMaskedInfo(this);
            }
            
            return gameModelsForPlayers;
        }

        internal GameModel SetMask(int team, bool[,] mapMask, int money, int movementPointsLeft)
        {
            for (int i = 0; i < length; i++)
            {

            }
        }
    }
}
