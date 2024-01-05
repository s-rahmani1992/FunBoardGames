using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;

namespace OnlineBoardGames.CantStop
{
    public class CantStopPlayer : BoardGamePlayer
    {
        [field: SyncVar( OnChange = nameof(OnTurnChanged))] 
        public bool IsTurn { get; private set; }

        [field: SyncVar(OnChange = nameof(OnDiceChanged))]
        public DiceData RolledDices { get; private set; } = DiceData.Empty();

        [field: SyncVar(OnChange = nameof(OnFinishedConeChenged))]
        public int FinishedConeCount { get; private set;}

        public PlayerColor PlayerColor => (PlayerColor)Index;

        [SyncObject]
        readonly SyncDictionary<int, int> conePositions = new();
        public IDictionary<int, int> ConePositions => conePositions;

        private void OnDiceChanged(DiceData _, DiceData value, bool __)
        {
            RollChanged?.Invoke(value);
        }

        private void OnFinishedConeChenged(int _, int value, bool __)
        {
            FinishedConeChanged?.Invoke(value);
        }

        public event Action TurnStart;
        public event Action TurnEnd;
        public event Action<DiceData> RollChanged;
        public event Action<CantStopPlayer, int, int> ConePositionChanged;
        public event Action<int> FinishedConeChanged;

        #region Server Events
        public event Action<CantStopPlayer> RollRequested;
        public event Action<CantStopPlayer, PlayerPlayData> Placed;
        public event Action<CantStopPlayer, PlayerPlayData> Played;
        #endregion

        private void OnTurnChanged(bool _, bool newValue, bool __)
        {
            if (newValue)
                TurnStart?.Invoke();
            else
                TurnEnd?.Invoke();
        }

        public IEnumerable<int> UpdateConePosition(IDictionary<int, int> poses, GameBoard board)
        {
            List<int> finishedColumns = new();

            foreach (var p in poses)
            {
                int g = -1;
                conePositions.TryGetValue(p.Key, out g);

                if (g != p.Value && p.Value == board[p.Key] - 1)
                    finishedColumns.Add(p.Key);

                conePositions[p.Key] = p.Value;
            }

            FinishedConeCount += finishedColumns.Count;

            return finishedColumns;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            conePositions.OnChange += ConePositions_OnChange;;
        }

        private void ConePositions_OnChange(SyncDictionaryOperation op, int key, int item, bool _)
        {
            if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
            {
                ConePositionChanged?.Invoke(this, key, item);
            }
        }

        [Server]
        public void SetTurn(bool turn) => IsTurn = turn;

        [Server]
        public void SetRoll(DiceData rollData) => RolledDices = rollData;

        [ServerRpc]
        public void CmdRoll() => RollRequested?.Invoke(this);

        [ServerRpc]
        public void CmdPlace(PlayerPlayData data) => Placed?.Invoke(this, data);

        [ServerRpc]
        public void CmdPlay(PlayerPlayData data) => Played?.Invoke(this, data);
    }
}
