using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class CantStopUIManager : MonoBehaviour
    {
        CantStopRoomManager roomManager;

        [SerializeField] CantStopPlayerUI[] playerUis;
        [SerializeField] Color[] playerColors;

        private void Awake()
        {
            roomManager = FindObjectOfType<CantStopRoomManager>();
            roomManager.LocalPlayer.CmdGameReady();
            Subscribe();
        }

        void Subscribe()
        {
            roomManager.GameBegin += OnGameBegin;
        }

        private void OnGameBegin()
        {
            foreach(var player in roomManager.Players)
            {
                playerUis[player.Index - 1].SetPlayer(player as CantStopPlayer, playerColors[player.Index - 1]);
            }
        }
    }
}
