using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.CantStop
{
    public class CantStopUIManager : MonoBehaviour
    {
        CantStopRoomManager roomManager;

        [SerializeField] CantStopPlayerUI[] playerUis;
        [SerializeField] Color[] playerColors;
        [SerializeField] Button rollButton;
        [SerializeField] DiceController diceController;

        private void Awake()
        {
            rollButton.interactable = false;
            roomManager = FindObjectOfType<CantStopRoomManager>();
            roomManager.LocalPlayer.CmdGameReady();
            Subscribe();
        }

        void Subscribe()
        {
            roomManager.GameBegin += OnGameBegin;
            roomManager.TurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(CantStopPlayer player)
        {
            rollButton.interactable = player.hasAuthority;
        }

        private void OnRollChanged(DiceData data)
        {
            if (!data.IsValid)
            {
                diceController.Clear();
                return;
            }

            diceController.SetDiceValues(data);
        }

        private void OnGameBegin()
        {
            foreach(var player in roomManager.PlayerList)
            {
                playerUis[player.Index - 1].SetPlayer(player, playerColors[player.Index - 1]);
                player.RollChanged += OnRollChanged;
            }
        }

        public void RollClicked()
        {
            (roomManager.LocalPlayer as CantStopPlayer).CmdRoll();
        }
    }
}
