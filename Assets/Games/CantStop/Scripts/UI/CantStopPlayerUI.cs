using FunBoardGames.Client;
using FunBoardGames.Network;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class CantStopPlayerUI : MonoBehaviour
    {
        [SerializeField] Text nameText;
        [SerializeField] Image coneIcon;
        [SerializeField] Transform finishHolder;
        [SerializeField] ProgressTimer timer;
        [SerializeField] Transform timeoutHolder;
        [SerializeField] Sprite timeoutActiveSprite;
        [SerializeField] Sprite timeoutInactiveSprite;
        [SerializeField] Color emptyConeColor = new(1f, 1f, 1f, 0.25f);
        [SerializeField] RawImage turnLED;
        [SerializeField] Texture2D onTex, offTex;
        [SerializeField] CantStopAssetManager assetManager;
        [SerializeField] AvatarAssetManager avatarAssets;
        [SerializeField] Image avatarImage;
        [SerializeField] Image namePlate;
        [SerializeField] Color nameTextColor = new(0.13f, 0.07f, 0f);
        [SerializeField] Color myPlateColor = Color.white;
        [SerializeField] Color otherPlateColor = new(0.8f, 0.86f, 0.95f);

        ICantStopPlayer networkPlayer;
        Color playerColor;
        readonly List<Image> finishCones = new();
        readonly List<Image> timeoutIcons = new();

        public void SetPlayer(ICantStopPlayer player, int winConeCount, int timeoutLimit)
        {
            playerColor = assetManager.GetPlayerColor(player.ConeColor);
            networkPlayer = player;
            CreateFinishCones(winConeCount);
            CreateTimeoutIcons(timeoutLimit);
            RefreshUI();
        }

        public void RefreshUI()
        {
            nameText.color = nameTextColor;
            nameText.text = networkPlayer.Profile.PlayerName;

            if (namePlate != null)
                namePlate.color = networkPlayer.IsMe ? myPlateColor : otherPlateColor;

            if (avatarImage != null && avatarAssets != null)
                avatarImage.sprite = avatarAssets.GetAvatar(networkPlayer.Profile.Avatar);

            coneIcon.color = playerColor;
            UpdateScore(networkPlayer.Score);
            UpdateTimeouts();
            gameObject.SetActive(true);
        }

        public void ToggleTurn(bool isTurn)
        {
            turnLED.texture = (isTurn ? onTex : offTex);
        }

        public void StartTimer(float duration, float elapsed)
        {
            timer.gameObject.SetActive(true);
            timer.StartTimer(duration, elapsed);
        }

        public void StopTimer() => timer.Stop();

        public void HideTimer()
        {
            timer.Clear();
            timer.gameObject.SetActive(false);
        }

        internal void UpdateTimeouts()
        {
            int left = timeoutIcons.Count - networkPlayer.TimeoutCount;

            for (int i = 0; i < timeoutIcons.Count; i++)
                timeoutIcons[i].sprite = (i < left ? timeoutActiveSprite : timeoutInactiveSprite);
        }

        // Cones light up from left to right, one for each finished column
        internal void UpdateScore(int score)
        {
            for (int i = 0; i < finishCones.Count; i++)
                finishCones[i].color = (i < score ? playerColor : emptyConeColor);
        }

        void CreateTimeoutIcons(int timeoutLimit)
        {
            timeoutIcons.Clear();
            var template = timeoutHolder.GetChild(0).gameObject;

            while (timeoutHolder.childCount < timeoutLimit)
                Instantiate(template, timeoutHolder);

            for (int i = 0; i < timeoutHolder.childCount; i++)
            {
                var icon = timeoutHolder.GetChild(i);
                bool isUsed = i < timeoutLimit;
                icon.gameObject.SetActive(isUsed);

                if (isUsed)
                    timeoutIcons.Add(icon.GetComponent<Image>());
            }
        }

        void CreateFinishCones(int winConeCount)
        {
            finishCones.Clear();
            var template = finishHolder.GetChild(0).gameObject;

            while (finishHolder.childCount < winConeCount)
                Instantiate(template, finishHolder);

            for (int i = 0; i < finishHolder.childCount; i++)
            {
                var cone = finishHolder.GetChild(i);
                bool isUsed = i < winConeCount;
                cone.gameObject.SetActive(isUsed);

                if (isUsed)
                    finishCones.Add(cone.GetComponent<Image>());
            }
        }
    }
}
