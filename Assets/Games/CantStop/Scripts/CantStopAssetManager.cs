using UnityEngine;

namespace FunBoardGames.CantStop
{
    [CreateAssetMenu]
    public class CantStopAssetManager : ScriptableObject
    {
        [SerializeField] Color[] playerColors;

        public Color GetPlayerColor(PlayerColor playerColor)
        {
            return playerColors[(int)playerColor];
        }
    }
}
