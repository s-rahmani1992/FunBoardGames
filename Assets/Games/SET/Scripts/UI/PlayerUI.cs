using FunBoardGames.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] Text playerTxt, scoreTxt;
        [SerializeField] TMP_Text statusTxt;
        [SerializeField] RawImage LED;
        [SerializeField] Texture2D ledOn, ledOff;
        [SerializeField] Transform wrongHolder;
        [SerializeField] Texture2D wrongLedOn, wrongLedOff;
        [SerializeField] Color playingColor = new(0f, 1f, 0.1424f);
        [SerializeField] Color inactiveColor = Color.red;

        ISETPlayer networkPlayer;
        readonly List<RawImage> wrongLEDs = new();

        public void UpdateScores()
        {
            scoreTxt.text = networkPlayer.CorrectScore.ToString();

            // LEDs light up from left to right, one for each wrong score
            for (int i = 0; i < wrongLEDs.Count; i++)
                wrongLEDs[i].texture = (i < networkPlayer.WrongScore ? wrongLedOn : wrongLedOff);

            RefreshStatus();
        }

        public void RefreshStatus()
        {
            if (networkPlayer.HasLeft)
                SetStatus("Left", inactiveColor);
            else if (networkPlayer.IsBusted)
                SetStatus("Busted", inactiveColor);
            else
                SetStatus("Playing", playingColor);
        }

        void SetStatus(string status, Color color)
        {
            statusTxt.text = status;
            statusTxt.color = color;
        }

        private void PlayerLeft()
        {
            ToggleGuess(false);
            RefreshStatus();
        }

        public void SetPlayer(ISETPlayer player, int wrongLimit)
        {
            if (networkPlayer != null)
                UnSubscribe();

            networkPlayer = player;
            playerTxt.color = (networkPlayer.IsMe ? Color.yellow : Color.cyan);
            playerTxt.text = networkPlayer.Name;
            CreateWrongLEDs(wrongLimit);
            gameObject.SetActive(true);
            UpdateScores();
            ToggleGuess(false);
            Subscribe();
        }

        void CreateWrongLEDs(int wrongLimit)
        {
            wrongLEDs.Clear();
            var ledTemplate = wrongHolder.GetChild(0).gameObject;

            while (wrongHolder.childCount < wrongLimit)
                Instantiate(ledTemplate, wrongHolder);

            for (int i = 0; i < wrongHolder.childCount; i++)
            {
                var led = wrongHolder.GetChild(i);
                bool isUsed = i < wrongLimit;
                led.gameObject.SetActive(isUsed);

                if (isUsed)
                    wrongLEDs.Add(led.GetComponent<RawImage>());
            }
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
