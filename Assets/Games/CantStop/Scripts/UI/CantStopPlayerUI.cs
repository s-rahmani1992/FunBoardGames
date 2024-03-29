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

        CantStopPlayer networkPlayer;
        Color playerColor;

        public void SetPlayer(CantStopPlayer player, Color color)
        {
            if (networkPlayer != null)
                UnSubscribe();

            playerColor = color;
            networkPlayer = player;
            RefreshUI();
            Subscribe();
        }

        public void RefreshUI()
        {
            nameText.color = (networkPlayer.IsOwner ? Color.yellow : Color.cyan);
            nameText.text = networkPlayer.Name;
            coneIcon.color = playerColor;
            scoreText.text = networkPlayer.FinishedConeCount.ToString();
            gameObject.SetActive(true);
        }

        void Subscribe()
        {
            networkPlayer.TurnStart += OnTurnStart;
            networkPlayer.TurnEnd += OnTurnEnd;
            networkPlayer.FinishedConeChanged += OnFinishedConeChanged;
        }

        private void OnFinishedConeChanged(int value)
        {
            scoreText.text = value.ToString();
        }

        void UnSubscribe()
        {
            networkPlayer.TurnStart -= OnTurnStart;
            networkPlayer.TurnEnd -= OnTurnEnd;
            networkPlayer.FinishedConeChanged -= OnFinishedConeChanged;
        }

        private void OnTurnEnd() => turnLED.texture = offTex;

        private void OnTurnStart() => turnLED.texture = onTex;
    }
}
