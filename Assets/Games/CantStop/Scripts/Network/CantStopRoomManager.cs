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

        [field: SyncVar]
        public GameBoard Board { get; private set; }

#if UNITY_EDITOR
        [SerializeField] GameBoard board;
#endif

        private void OnTurnStarted(CantStopPlayer _, CantStopPlayer player)
        {
            TurnStarted?.Invoke(player);
        }

        int turnIndex = 0;

        public IEnumerable<CantStopPlayer> PlayerList => Players.Select(p => p as CantStopPlayer);
        public bool IsYourTurn => LocalPlayer == CurrentTurnPlayer;

        public event Action<CantStopPlayer> TurnStarted;
        public event Action<int, int> TurnPlayed;

        protected override void BeginGame() 
        {
            SetTurn(Players[turnIndex] as CantStopPlayer);

            foreach(var player in PlayerList)
            {
                player.RollRequested += OnRollRequested;
                player.Played += OnPlayed;
            }
        }

        private async void OnPlayed(CantStopPlayer player, int index1, int index2)
        {
            if (player != CurrentTurnPlayer)
                return;

            RpcTurnPlayed(index1, index2);
            await Task.Delay(2000);
            turnIndex = (turnIndex + 1) % Players.Count();
            SetTurn(Players[turnIndex] as CantStopPlayer);
        }

        private void OnRollRequested(CantStopPlayer player)
        {
            if (player != CurrentTurnPlayer)
                return;

            player.SetRoll(DiceData.Default());
        }

        [Server]
        void SetTurn(CantStopPlayer player)
        {
            if (CurrentTurnPlayer != null)
                CurrentTurnPlayer.SetTurn(false);

            player.SetTurn(true);
            CurrentTurnPlayer = player;
        }

        [ClientRpc(includeOwner = false)]
        void RpcTurnPlayed(int index1, int index2)
        {
            TurnPlayed?.Invoke(index1, index2);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
#if UNITY_EDITOR
            if (GameNetworkManager.singleton.OverrideGame)
                Board = board;
            else
                Board = BoardGameCardDataHolder.Instance.Board;
#else
            Board = BoardGameCardDataHolder.Instance.Board;
#endif
        }
    }
}
