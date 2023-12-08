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

        SyncDictionary<int, int> whiteConePositions = new();
        public IDictionary<int, int> WhiteConePositions => whiteConePositions;

        public const int whineConeLimit = 3;

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
        public event Action<int, int> WhiteConeMoved;
        public event Action<int, int> DiceSelected;

        protected override void BeginGame() 
        {
            SetTurn(Players[turnIndex] as CantStopPlayer);

            foreach(var player in PlayerList)
            {
                player.RollRequested += OnRollRequested;
                player.Placed += OnPlaced;
            }
        }

        private async void OnPlaced(CantStopPlayer player, int diceIndex1, int diceIndex2, int index1, int? index2)
        {
            if (player != CurrentTurnPlayer)
                return;

            RpcDiceSelect(diceIndex1, diceIndex2);

            bool single = 2 * (player.RolledDices[diceIndex1] + player.RolledDices[diceIndex2]) == player.RolledDices.Sum;

            if (whiteConePositions.TryGetValue(index1, out int c1))
                whiteConePositions[index1] = c1 + (single? 2 : 1);
            else
                whiteConePositions.Add(index1, c1 + (single ? 1 : 0));

            if (index2 != null)
            {
                if (whiteConePositions.TryGetValue(index2.Value, out int cx))
                    whiteConePositions[index2.Value] = cx + 1;
                else
                    whiteConePositions.Add(index2.Value, 0);
            }


            await Task.Delay(2000);
            player.SetRoll(DiceData.Empty());
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
        void RpcDiceSelect(int index1, int index2)
        {
            DiceSelected?.Invoke(index1, index2);
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

        public override void OnStartClient()
        {
            base.OnStartClient();
            whiteConePositions.Callback += OnWhiteConePositions_Callback;
        }

        private void OnWhiteConePositions_Callback(SyncIDictionary<int, int>.Operation op, int key, int item)
        {
            if(op == SyncIDictionary<int, int>.Operation.OP_ADD || op == SyncIDictionary<int, int>.Operation.OP_SET)
            {
                WhiteConeMoved?.Invoke(key, item);
            }
        }
    }
}
