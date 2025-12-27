using FunBoardGames.Network;

using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] Text playerTxt, correctTxt, wrongTxt, scoreTxt;
        [SerializeField] RawImage LED;
        [SerializeField] Texture2D ledOn, ledOff;

        ISETPlayer networkPlayer;

        public void UpdateScores()
        {
            correctTxt.text = networkPlayer.CorrectScore.ToString();
            wrongTxt.text = networkPlayer.WrongScore.ToString();
            scoreTxt.text = (networkPlayer.CorrectScore - networkPlayer.WrongScore).ToString();
        }

        private void PlayerLeft()
        {
            playerTxt.text = "Player Left";
        }

        internal void RefreshVote(VoteAnswer newVal){
            //voteUI?.UpdateUI(newVal);
        }

        public void SetPlayer(ISETPlayer player)
        {
            if (networkPlayer != null)
                UnSubscribe();

            networkPlayer = player;
            playerTxt.color = (networkPlayer.IsMe ? Color.yellow : Color.cyan);
            playerTxt.text = networkPlayer.Name;
            gameObject.SetActive(true);
            UpdateScores();
            ToggleGuess(false);
            Subscribe();
        }

        void Subscribe()
        {
            networkPlayer.LeftGame += PlayerLeft;
        }

        public void ToggleGuess(bool isOn)
        {
            LED.texture = (isOn ? ledOn : ledOff);
        }

        void UnSubscribe()
        {
            networkPlayer.LeftGame -= PlayerLeft;
        }

        private void OnDestroy()
        {
            if (networkPlayer == null)
                return;

            UnSubscribe();
        }
    }
}