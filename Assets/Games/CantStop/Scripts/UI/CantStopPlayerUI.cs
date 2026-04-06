using FunBoardGames.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class CantStopPlayerUI : MonoBehaviour
    {
        [SerializeField] Text nameText;
        [SerializeField] Image coneIcon;
        [SerializeField] Text scoreText;
        [SerializeField] RawImage turnLED;
        [SerializeField] Texture2D onTex, offTex;
        [SerializeField] CantStopAssetManager assetManager;

        ICantStopPlayer networkPlayer;
        Color playerColor;

        public void SetPlayer(ICantStopPlayer player)
        {
            playerColor = assetManager.GetPlayerColor(player.ConeColor);
            networkPlayer = player;
            RefreshUI();
        }

        public void RefreshUI()
        {
            nameText.color = (networkPlayer.IsMe ? Color.yellow : Color.cyan);
            nameText.text = networkPlayer.Name;
            coneIcon.color = playerColor;
            scoreText.text = networkPlayer.Score.ToString();
            gameObject.SetActive(true);
        }

        public void ToggleTurn(bool isTurn)
        {
            turnLED.texture = (isTurn ? onTex : offTex);
        }

        internal void UpdateScore(int score)
        {
            scoreText.text = score.ToString();
        }
    }
}
