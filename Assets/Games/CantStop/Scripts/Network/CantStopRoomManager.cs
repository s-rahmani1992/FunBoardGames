using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class CantStopRoomManager : RoomManager
    {
        public override BoardGame GameType => BoardGame.CantStop;

        [field: SyncVar] public CantStopPlayer CurrentTurnPlayer { get; private set; }

        int turnIndex = 0;

        protected override void BeginGame() 
        {
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
