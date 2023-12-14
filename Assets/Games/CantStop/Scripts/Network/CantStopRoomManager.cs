using Mirror;
using System;
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
                player.Played += OnPlayed;
            }
        }

        private async void OnPlayed(CantStopPlayer player, PlayerPlayData data)
        {
            if (player != CurrentTurnPlayer)
                return;

            if(data == null)
            {
                await Task.Delay(2000);
                turnIndex = (turnIndex + 1) % Players.Count();
                SetTurn(Players[turnIndex] as CantStopPlayer);
                return;
            }

            RpcDiceSelect(data.DiceIndex1, data.DiceIndex2);

            bool single = 2 * (player.RolledDices[data.DiceIndex1] + player.RolledDices[data.DiceIndex2]) == player.RolledDices.Sum;

            if (whiteConePositions.TryGetValue(data.ColumnIndex1, out int c1))
                whiteConePositions[data.ColumnIndex1] = c1 + (single ? 2 : 1);
            else
            {
                if (player.ConePositions.TryGetValue(data.ColumnIndex1, out c1))
                    whiteConePositions.Add(data.ColumnIndex1, c1 + (single ? 2 : 1));
                else
                    whiteConePositions.Add(data.ColumnIndex1, single ? 1 : 0);
            }

            if (data.ColumnIndex2 != null)
            {
                if (whiteConePositions.TryGetValue(data.ColumnIndex2.Value, out int cx))
                    whiteConePositions[data.ColumnIndex2.Value] = cx + 1;
                else
                {
                    if (player.ConePositions.TryGetValue(data.ColumnIndex2.Value, out cx))
                        whiteConePositions.Add(data.ColumnIndex2.Value, cx + (single ? 2 : 1));
                    else
                        whiteConePositions.Add(data.ColumnIndex2.Value, single ? 1 : 0);
                }
            }
            await Task.Delay(2000);
            player.UpdateConePosition(whiteConePositions);
            await Task.Delay(2000);
            turnIndex = (turnIndex + 1) % Players.Count();
            SetTurn(Players[turnIndex] as CantStopPlayer);
        }

        private async void OnPlaced(CantStopPlayer player, PlayerPlayData data)
        {
            if (player != CurrentTurnPlayer)
                return;

            RpcDiceSelect(data.DiceIndex1, data.DiceIndex2);

            bool single = 2 * (player.RolledDices[data.DiceIndex1] + player.RolledDices[data.DiceIndex2]) == player.RolledDices.Sum;

            if (whiteConePositions.TryGetValue(data.ColumnIndex1, out int c1))
                whiteConePositions[data.ColumnIndex1] = c1 + (single ? 2 : 1);
            else
            {
                if(player.ConePositions.TryGetValue(data.ColumnIndex1, out c1))
                    whiteConePositions.Add(data.ColumnIndex1, c1 + (single ? 2 : 1));
                else
                    whiteConePositions.Add(data.ColumnIndex1, single ? 1 : 0);
            }

            if (data.ColumnIndex2 != null)
            {
                if (whiteConePositions.TryGetValue(data.ColumnIndex2.Value, out int cx))
                    whiteConePositions[data.ColumnIndex2.Value] = cx + 1;
                else
                {
                    if (player.ConePositions.TryGetValue(data.ColumnIndex2.Value, out cx))
                        whiteConePositions.Add(data.ColumnIndex2.Value, cx + (single ? 2 : 1));
                    else
                        whiteConePositions.Add(data.ColumnIndex2.Value, single ? 1 : 0);
                }
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
            whiteConePositions.Clear();
            player.SetRoll(DiceData.Empty());
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
