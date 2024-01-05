using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace OnlineBoardGames.SET
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] Text playerTxt, correctTxt, wrongTxt, scoreTxt;
        [SerializeField] RawImage LED;
        [SerializeField] Texture2D ledOn, ledOff;

        SETPlayer networkPlayer;

        public void RefreshUI(){
            playerTxt.color = (networkPlayer.IsOwner ? Color.yellow : Color.cyan);
            playerTxt.text = networkPlayer.Name;
            correctTxt.text = networkPlayer.CorrectCount.ToString();
            wrongTxt.text = networkPlayer.WrongCount.ToString();
            scoreTxt.text = (networkPlayer.CorrectCount - networkPlayer.WrongCount).ToString();
            gameObject.SetActive(true);
            LED.texture = (networkPlayer.IsGuessing ? ledOn : ledOff);
        }

        public void PlayerLeft()
        {
            playerTxt.text = "Player Left";
        }

        internal void RefreshVote(VoteAnswer newVal){
            //voteUI?.UpdateUI(newVal);
        }

        public void SetPlayer(SETPlayer player)
        {
            if (networkPlayer != null)
                UnSubscribe();

            networkPlayer = player;
            RefreshUI();
            Subscribe();
        }

        void Subscribe()
        {
            networkPlayer.WrongGuessChanged += OnWrongGuessChanged;
            networkPlayer.CorrectGuessChanged += OnCorrectGuessChanged;
            networkPlayer.GuessChanged += OnGuessChanged;
            networkPlayer.LeftGame += PlayerLeft;
        }

        private void OnGuessChanged(bool isOn)
        {
            LED.texture = (isOn ? ledOn : ledOff);
        }

        void UnSubscribe()
        {
            networkPlayer.WrongGuessChanged -= OnWrongGuessChanged;
            networkPlayer.CorrectGuessChanged -= OnCorrectGuessChanged;
            networkPlayer.GuessChanged -= OnGuessChanged;
            networkPlayer.LeftGame -= PlayerLeft;
        }

        private void OnCorrectGuessChanged(int _, int newValue)
        {
            correctTxt.text = newValue.ToString();
            scoreTxt.text = (newValue - networkPlayer.WrongCount).ToString();
        }

        private void OnWrongGuessChanged(int _, int newValue)
        {
            wrongTxt.text = newValue.ToString();
            scoreTxt.text = (networkPlayer.CorrectCount - newValue).ToString();
        }

        private void OnDestroy()
        {
            if (networkPlayer == null)
                return;

            UnSubscribe();
        }
    }
}