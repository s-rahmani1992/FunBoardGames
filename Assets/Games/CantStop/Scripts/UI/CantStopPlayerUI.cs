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

        public void SetPlayer(ICantStopPlayer player, int winConeCount)
        {
            playerColor = assetManager.GetPlayerColor(player.ConeColor);
            networkPlayer = player;
            CreateFinishCones(winConeCount);
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
            gameObject.SetActive(true);
        }

        public void ToggleTurn(bool isTurn)
        {
            turnLED.texture = (isTurn ? onTex : offTex);
        }

        // Cones light up from left to right, one for each finished column
        internal void UpdateScore(int score)
        {
            for (int i = 0; i < finishCones.Count; i++)
                finishCones[i].color = (i < score ? playerColor : emptyConeColor);
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
