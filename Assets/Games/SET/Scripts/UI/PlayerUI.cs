using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace OnlineBoardGames.SET
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        Text playerTxt, correctTxt, wrongTxt, scoreTxt;
        [SerializeField]
        RawImage LED;
        [SerializeField]
        Texture2D ledOn, ledOff;

        public void RefreshUI(PlayerData data){
            playerTxt.text = data.name;
            correctTxt.text = data.correct.ToString();
            wrongTxt.text = data.wrong.ToString();
            scoreTxt.text = (data.correct - data.wrong).ToString();
            gameObject.SetActive(true);
            LED.texture = (data.isGuessing ? ledOn : ledOff);
            //voteUI.UpdateText(data.name);
        }

        public void PlayerLeft(){
            playerTxt.text = "Player Left";
            //voteUI?.gameObject.SetActive(false);
        }

        internal void RefreshVote(VoteStat newVal){
            //voteUI?.UpdateUI(newVal);
        }
    }
}