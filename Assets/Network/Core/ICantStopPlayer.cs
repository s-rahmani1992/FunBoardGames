
using UnityEngine;

namespace FunBoardGames.Network
{
    public interface ICantStopPlayer : IBoardGamePlayer
    {
        Color PlayerColor { get; }
    }
}
