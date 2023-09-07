using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] Button playButton;
        [SerializeField] DiceController diceController;
        [SerializeField] GameBoardUI boardController;

        private void Awake()
        {
            rollButton.interactable = false;
            playButton.interactable = false;
            roomManager = FindObjectOfType<CantStopRoomManager>();
            roomManager.LocalPlayer.CmdGameReady();
            diceController.PairSelected += (pair) =>
            {
                if (!roomManager.IsYourTurn)
                    return;

                playButton.interactable = pair;
            };
            Subscribe();
        }

        void Subscribe()
        {
            Unsubscribe();
            roomManager.GameBegin += OnGameBegin;
            roomManager.TurnStarted += OnTurnStarted;
            roomManager.TurnPlayed += OnTurnPlayed;
        }

        void Unsubscribe()
        {
            roomManager.GameBegin -= OnGameBegin;
            roomManager.TurnStarted -= OnTurnStarted;
            roomManager.TurnPlayed -= OnTurnPlayed;
        }

        private void OnTurnPlayed(int index1, int index2)
        {
            diceController.PickDices(index1, index2);
        }

        private void OnTurnStarted(CantStopPlayer player)
        {
            rollButton.interactable = player.hasAuthority;
            diceController.Reset();
        }

        private void OnRollChanged(DiceData data)
        {
            if (!data.IsValid)
            {
                diceController.ClearDices();
                return;
            }

            diceController.SetDiceValues(data);
        }

        private void OnGameBegin()
        {
            boardController.Initialize(roomManager.Board);

            foreach(var player in roomManager.PlayerList)
            {
                playerUis[player.Index - 1].SetPlayer(player, playerColors[player.Index - 1]);
                player.RollChanged += OnRollChanged;
            }
        }

        public void RollClicked()
        {
            rollButton.interactable = false;
            (roomManager.LocalPlayer as CantStopPlayer).CmdRoll();
        }

        public void PlayClicked()
        {
            var indices = diceController.SelectedIndices.ToList();
            (roomManager.LocalPlayer as CantStopPlayer).CmdPlay(indices[0], indices[1]);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
