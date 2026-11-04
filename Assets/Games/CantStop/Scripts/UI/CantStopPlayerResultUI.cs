using FunBoardGames.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class CantStopPlayerResultUI : MonoBehaviour
    {
        [SerializeField] Text rankText;
        [SerializeField] Text nameText;
        [SerializeField] Text scoreText;

        public void SETUI(ICantStopPlayer player, int rank)
        {
            rankText.text = rank.ToString();
            nameText.text = player.HasLeft ? player.Profile.PlayerName + " (Left)" : player.Profile.PlayerName;
            scoreText.text = player.Score.ToString();
        }
    }
}
