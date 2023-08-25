using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.CullingGroup;

namespace OnlineBoardGames.CantStop
{
    public class CantStopUIManager : MonoBehaviour
    {
        CantStopRoomManager roomManager;

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
            Debug.Log("Game Cant stop Begin");
        }
    }
}
