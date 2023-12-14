using Mirror;
using System;
using System.Collections.Generic;

namespace OnlineBoardGames.CantStop
{
    public class CantStopPlayer : BoardGamePlayer
    {
        [field: SyncVar] public int Score { get; private set; }

        [field: SyncVar] public int FreeConeCount { get; private set; }

        SyncDictionary<int, int> conePositions = new();

        [field: SyncVar( hook = nameof(OnTurnChanged))] 
        public bool IsTurn { get; private set; }

        [field: SyncVar(hook = nameof(OnDiceChanged))]
        public DiceData RolledDices { get; private set; } = DiceData.Empty();

        public PlayerColor PlayerColor => (PlayerColor)Index;
        public IDictionary<int, int> ConePositions => conePositions;

        private void OnDiceChanged(DiceData _, DiceData value)
        {
            RollChanged?.Invoke(value);
        }

        public event Action TurnStart;
        public event Action TurnEnd;
        public event Action<DiceData> RollChanged;
        public event Action<CantStopPlayer, int, int> ConePositionChanged;

        #region Server Events
        public event Action<CantStopPlayer> RollRequested;
        public event Action<CantStopPlayer, PlayerPlayData> Placed;
        public event Action<CantStopPlayer, PlayerPlayData> Played;
        #endregion

        private void OnTurnChanged(bool _, bool newValue)
        {
            if (newValue)
                TurnStart?.Invoke();
            else
                TurnEnd?.Invoke();
        }

        public void UpdateConePosition(IDictionary<int, int> poses)
        {
            foreach (var p in poses)
                conePositions[p.Key] = p.Value; 
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            FreeConeCount = 12;
            Score = 0;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            conePositions.Callback += ConePositions_Callback;
        }

        private void ConePositions_Callback(SyncIDictionary<int, int>.Operation op, int key, int item)
        {
            if (op == SyncIDictionary<int, int>.Operation.OP_ADD || op == SyncIDictionary<int, int>.Operation.OP_SET)
            {
                ConePositionChanged?.Invoke(this, key, item);
            }
        }

        [Server]
        public void SetTurn(bool turn) => IsTurn = turn;

        [Server]
        public void SetRoll(DiceData rollData) => RolledDices = rollData;

        [Command]
        public void CmdRoll() => RollRequested?.Invoke(this);

        [Command]
        public void CmdPlace(PlayerPlayData data) => Placed?.Invoke(this, data);

        [Command]
        public void CmdPlay(PlayerPlayData data) => Played?.Invoke(this, data);
    }
}
