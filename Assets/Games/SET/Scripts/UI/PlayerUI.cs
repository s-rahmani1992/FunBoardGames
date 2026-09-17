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
        [SerializeField] Transform hintHolder;
        [SerializeField] Color usedHintColor = Color.gray;
        [SerializeField] Color playingColor = new(0f, 1f, 0.1424f);
        [SerializeField] Color inactiveColor = Color.red;

        ISETPlayer networkPlayer;
        readonly List<RawImage> wrongLEDs = new();
        readonly List<RawImage> hintIcons = new();
        Color hintIconColor = Color.white;
        int? hintLimit;

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

        public void UseHint()
        {
            // The single icon of an unlimited game only shows the hint of the current round
            SetUsedHintIcons(hintLimit == null ? 1 : networkPlayer.UsedHintCount);
        }

        /// <summary>
        /// Frees the hint icon of an unlimited game, since the player can use a hint again in the new round.
        /// </summary>
        public void ResetRoundHint()
        {
            if (hintLimit == null)
                SetUsedHintIcons(0);
        }

        void SetUsedHintIcons(int usedCount)
        {
            // Hint icons are used from left to right, one for each hint the player used
            for (int i = 0; i < hintIcons.Count; i++)
            {
                bool isUsed = i < usedCount;
                hintIcons[i].color = (isUsed ? usedHintColor : hintIconColor);
                hintIcons[i].transform.GetChild(0).gameObject.SetActive(isUsed);
            }
        }

        public void SetPlayer(ISETPlayer player, int wrongLimit, int? hintLimit)
        {
            if (networkPlayer != null)
                UnSubscribe();

            networkPlayer = player;
            playerTxt.color = (networkPlayer.IsMe ? Color.yellow : Color.cyan);
            playerTxt.text = networkPlayer.Name;
            CreateWrongLEDs(wrongLimit);
            CreateHintIcons(hintLimit);
            gameObject.SetActive(true);
            UpdateScores();
            SetUsedHintIcons(0);
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

        void CreateHintIcons(int? hintLimit)
        {
            this.hintLimit = hintLimit;
            hintIcons.Clear();

            var iconTemplate = hintHolder.GetChild(0).gameObject;
            hintIconColor = iconTemplate.GetComponent<RawImage>().color;

            // Without a limit, the one icon of the prefab shows whether the player used a hint in the current round
            int iconCount = hintLimit ?? 1;

            while (hintHolder.childCount < iconCount)
                Instantiate(iconTemplate, hintHolder);

            for (int i = 0; i < hintHolder.childCount; i++)
            {
                var icon = hintHolder.GetChild(i);
                bool isUsed = i < iconCount;
                icon.gameObject.SetActive(isUsed);

                if (isUsed)
                    hintIcons.Add(icon.GetComponent<RawImage>());
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
