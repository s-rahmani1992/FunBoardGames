using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace OnlineBoardGames
{
    public class RoomPlayerUI : MonoBehaviour
    {
        [SerializeField]
        Text playerTxt;
        [SerializeField]
        RawImage readyLED;
        [SerializeField]
        Texture2D ledOn, ledOff;

        public void RefreshUI(string name, bool ready){
            playerTxt.text = name;
            readyLED.texture = (ready ? ledOn : ledOff);
        }

        public void PlayerLeft(){
            gameObject.SetActive(false);
        }

    }
}