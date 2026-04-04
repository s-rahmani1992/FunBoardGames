
using System.Collections.Generic;
using UnityEngine;

namespace FunBoardGames.Network
{
    public interface ICantStopPlayer : IBoardGamePlayer
    {
        Color PlayerColor { get; }

        IDictionary<int, int> ConePositions { get; }
    }
}
