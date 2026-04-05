
using FunBoardGames.CantStop;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface ICantStopPlayer : IBoardGamePlayer
    {
        PlayerColor ConeColor { get; }

        IDictionary<int, int> ConePositions { get; }
    }
}
