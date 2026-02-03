using FunBoardGames.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET {
    public class SETPlayerUIResult : MonoBehaviour
    {
        [SerializeField] Text rankText;
        [SerializeField] Text nameText;
        [SerializeField] Text correctText;
        [SerializeField] Text wrongText;
        [SerializeField] Text scoreText;

        public void SETUI(ISETPlayer player, int rank)
        {
            rankText.text = rank.ToString();
            nameText.text = player.Name;
            correctText.text = player.CorrectScore.ToString();
            wrongText.text = player.WrongScore.ToString();
            scoreText.text = (player.CorrectScore - player.WrongScore).ToString();
        }
    } 
}
