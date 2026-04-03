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

        ICantStopPlayer networkPlayer;
        Color playerColor;

        public void SetPlayer(ICantStopPlayer player)
        {
            if (networkPlayer != null)
                UnSubscribe();

            playerColor = player.PlayerColor;
            networkPlayer = player;
            RefreshUI();
            Subscribe();
        }

        public void RefreshUI()
        {
            nameText.color = (networkPlayer.IsMe ? Color.yellow : Color.cyan);
            nameText.text = networkPlayer.Name;
            coneIcon.color = playerColor;
            //scoreText.text = networkPlayer.FinishedConeCount.ToString();
            gameObject.SetActive(true);
        }

        void Subscribe()
        {
            //networkPlayer.TurnStart += OnTurnStart;
            //networkPlayer.TurnEnd += OnTurnEnd;
            //networkPlayer.FinishedConeChanged += OnFinishedConeChanged;
        }

        private void OnFinishedConeChanged(int value)
        {
            scoreText.text = value.ToString();
        }

        void UnSubscribe()
        {
            //networkPlayer.TurnStart -= OnTurnStart;
            //networkPlayer.TurnEnd -= OnTurnEnd;
            //networkPlayer.FinishedConeChanged -= OnFinishedConeChanged;
        }

        public void ToggleTurn(bool isTurn)
        {
            turnLED.texture = (isTurn ? onTex : offTex);
        }

        private void OnTurnEnd() => turnLED.texture = offTex;

        private void OnTurnStart() => turnLED.texture = onTex;
    }
}
