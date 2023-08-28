using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class CantStopPlayer : BoardGamePlayer
    {
        [field: SyncVar] public int Score { get; private set; }

        [field: SyncVar] public int FreeConeCount { get; private set; }

        [field: SyncVar( hook = nameof(OnTurnChanged))] 
        public bool IsTurn { get; private set; }

        public event Action TurnStart;
        public event Action TurnEnd;

        private void OnTurnChanged(bool _, bool newValue)
        {
            if (newValue)
                TurnStart?.Invoke();
            else
                TurnEnd?.Invoke();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            FreeConeCount = 12;
            Score = 0;
        }

        [Server]
        public void SetTurn(bool turn) => IsTurn = turn;
    }
}
