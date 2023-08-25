using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class CantStopPlayer : BoardGamePlayer
    {
        [field: SyncVar] public int Score { get; private set; }

        [field: SyncVar] public int FreeConeCount { get; private set; }

        public override void OnStartServer()
        {
            base.OnStartServer();
            FreeConeCount = 12;
            Score = 0;
        }
    }
}
