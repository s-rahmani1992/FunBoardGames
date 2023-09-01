using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class CantStopRoomManager : RoomManager
    {
        public override BoardGame GameType => BoardGame.CantStop;

        [field: SyncVar(hook = nameof(OnTurnStarted))] 
        public CantStopPlayer CurrentTurnPlayer { get; private set; }

        private void OnTurnStarted(CantStopPlayer _, CantStopPlayer player)
        {
            TurnStarted?.Invoke(player);
        }

        int turnIndex = 0;

        public IEnumerable<CantStopPlayer> PlayerList => Players.Select(p => p as CantStopPlayer);

        public event Action<CantStopPlayer> TurnStarted;

        protected override void BeginGame() 
        {
            SetTurn(Players[turnIndex] as CantStopPlayer);

            foreach(var player in PlayerList)
            {
                player.RollRequested += OnRollRequested;
            }
        }

        private async void OnRollRequested(CantStopPlayer player)
        {
            if (player != CurrentTurnPlayer)
                return;

            player.SetRoll(DiceData.Default());
            await Task.Delay(2000);
            turnIndex = (turnIndex + 1) % Players.Count();
            SetTurn(Players[turnIndex] as CantStopPlayer);
        }

        [Server]
        void SetTurn(CantStopPlayer player)
        {
            if (CurrentTurnPlayer != null)
                CurrentTurnPlayer.SetTurn(false);

            player.SetTurn(true);
            CurrentTurnPlayer = player;
        }
    }
}
