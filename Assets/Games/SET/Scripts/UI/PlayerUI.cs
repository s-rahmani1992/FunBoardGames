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

        SETNetworkPlayer networkPlayer;

        public void RefreshUI(){
            playerTxt.color = (networkPlayer.hasAuthority ? Color.yellow : Color.cyan);
            playerTxt.text = networkPlayer.playerName;
            correctTxt.text = networkPlayer.corrects.ToString();
            wrongTxt.text = networkPlayer.wrongs.ToString();
            scoreTxt.text = (networkPlayer.corrects - networkPlayer.wrongs).ToString();
            gameObject.SetActive(true);
            LED.texture = (networkPlayer.isGuessing ? ledOn : ledOff);
        }

        public void PlayerLeft()
        {
            playerTxt.text = "Player Left";
        }

        internal void RefreshVote(VoteStat newVal){
            //voteUI?.UpdateUI(newVal);
        }

        public void SetPlayer(SETNetworkPlayer player)
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
            scoreTxt.text = (newValue - networkPlayer.wrongs).ToString();
        }

        private void OnWrongGuessChanged(int _, int newValue)
        {
            wrongTxt.text = newValue.ToString();
            scoreTxt.text = (networkPlayer.corrects - newValue).ToString();
        }

        private void OnDestroy()
        {
            if (networkPlayer == null)
                return;

            UnSubscribe();
        }
    }
}