using FunBoardGames.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET {
    public class SETPlayerUIResult : MonoBehaviour
    {
        [SerializeField] Text rankText;
        [SerializeField] Text nameText;
        [SerializeField] Text statusText;
        [SerializeField] Text scoreText;

        public void SETUI(ISETPlayer player, int rank)
        {
            rankText.text = rank.ToString();
            nameText.text = player.Name;
            statusText.text = GetStatus(player);
            scoreText.text = player.CorrectScore.ToString();
        }

        static string GetStatus(ISETPlayer player)
        {
            if (player.HasLeft)
                return "Left";

            if (player.IsBusted)
                return "Busted";

            return string.Empty;
        }
    } 
}
