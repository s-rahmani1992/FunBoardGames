using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.CantStop
{
    public class CantStopPlayerUI : MonoBehaviour
    {
        [SerializeField] Text nameText;
        [SerializeField] Image coneIcon;
        [SerializeField] Text freeConeCount;
        [SerializeField] Text scoreText;

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
            nameText.color = (networkPlayer.hasAuthority ? Color.yellow : Color.cyan);
            nameText.text = networkPlayer.Name;
            coneIcon.color = playerColor;
            freeConeCount.text = networkPlayer.FreeConeCount.ToString();
            freeConeCount.color = playerColor;
            scoreText.text = networkPlayer.Score.ToString();
            gameObject.SetActive(true);
        }

        void Subscribe()
        {
            
        }

        void UnSubscribe()
        {
            
        }
    }
}
