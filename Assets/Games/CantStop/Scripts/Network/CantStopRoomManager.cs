using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FunBoardGames.CantStop
{
    public class CantStopRoomManager : RoomManager
    {
        public override BoardGame GameType => BoardGame.CantStop;

        [field: SyncVar(OnChange = nameof(OnTurnStarted))] 
        public CantStopPlayer CurrentTurnPlayer { get; private set; }

        [field: SyncVar]
        public GameBoard Board { get; private set; }

        [SyncObject]
        readonly SyncDictionary<int, int> whiteConePositions = new();
        public IDictionary<int, int> WhiteConePositions => whiteConePositions;

        [SyncObject]
        readonly SyncDictionary<int, CantStopPlayer> playerFinishPositions = new();
        public IDictionary<int, CantStopPlayer> PlayerFinishPositions => playerFinishPositions;

        public const int whineConeLimit = 3;

#if UNITY_EDITOR
        [SerializeField] GameBoard board;
#endif

        private void OnTurnStarted(CantStopPlayer _, CantStopPlayer player, bool __)
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
            SetTurn(Players.ElementAt(turnIndex) as CantStopPlayer);

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
                SetTurn(Players.ElementAt(turnIndex) as CantStopPlayer);
                return;
            }

            UpdateWhiteCones(player, data);
            await Task.Delay(2000);
            var g = player.UpdateConePosition(whiteConePositions, board);

            foreach (var n in g)
                PlayerFinishPositions.TryAdd(n, player);

            await Task.Delay(2000);
            turnIndex = (turnIndex + 1) % Players.Count();
            SetTurn(Players.ElementAt(turnIndex) as CantStopPlayer);
        }

        private async void OnPlaced(CantStopPlayer player, PlayerPlayData data)
        {
            if (player != CurrentTurnPlayer)
                return;

            UpdateWhiteCones(player, data);
            await Task.Delay(2000);
            player.SetRoll(DiceData.Empty());
        }

        [Server]
        void UpdateWhiteCones(CantStopPlayer player, PlayerPlayData data)
        {
            RpcDiceSelect(data.DiceIndex1, data.DiceIndex2);

            bool single = 2 * (player.RolledDices[data.DiceIndex1] + player.RolledDices[data.DiceIndex2]) == player.RolledDices.Sum;

            if (whiteConePositions.TryGetValue(data.ColumnIndex1, out int c1))
            {
                if(c1 < board[data.ColumnIndex1] - 1)
                    whiteConePositions[data.ColumnIndex1] = Mathf.Clamp(c1 + (single ? 2 : 1), 0, board[data.ColumnIndex1] - 1);
            }
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
                {
                    if (cx < board[data.ColumnIndex2.Value] - 1)
                        whiteConePositions[data.ColumnIndex2.Value] = c1 + 1;
                }
                else
                {
                    if (player.ConePositions.TryGetValue(data.ColumnIndex2.Value, out cx))
                        whiteConePositions.Add(data.ColumnIndex2.Value, cx + 1);
                    else
                        whiteConePositions.Add(data.ColumnIndex2.Value, 0);
                }
            }
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

        [ObserversRpc(ExcludeOwner = true)]
        void RpcDiceSelect(int index1, int index2)
        {
            DiceSelected?.Invoke(index1, index2);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
#if UNITY_EDITOR
            Board = board;
#else
            Board = BoardGameCardDataHolder.Instance.Board;
#endif
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            whiteConePositions.OnChange += WhiteConePositions_OnChange;
        }

        private void WhiteConePositions_OnChange(SyncDictionaryOperation op, int key, int item, bool asServer)
        {
            if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
            {
                WhiteConeMoved?.Invoke(key, item);
            }
        }
    }
}
