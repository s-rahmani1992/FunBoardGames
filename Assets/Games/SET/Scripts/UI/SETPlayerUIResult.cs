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

        public void SETUI(SETPlayer player, int rank)
        {
            rankText.text = rank.ToString();
            nameText.text = player.Name;
            correctText.text = player.CorrectCount.ToString();
            wrongText.text = player.WrongCount.ToString();
            scoreText.text = player.Score.ToString();
            gameObject.SetActive(true);
        }
    } 
}
