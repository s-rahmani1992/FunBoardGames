using Mirror;
using System;

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
        
        private void OnDiceChanged(DiceData _, DiceData value)
        {
            RollChanged?.Invoke(value);
        }

        public event Action TurnStart;
        public event Action TurnEnd;
        public event Action<DiceData> RollChanged;

        #region Server Events
        public event Action<CantStopPlayer> RollRequested;
        public event Action<CantStopPlayer,int, int, int, int?> Placed;
        #endregion

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

        [Server]
        public void SetRoll(DiceData rollData) => RolledDices = rollData;

        [Command]
        public void CmdRoll() => RollRequested?.Invoke(this);

        [Command]
        public void CmdPlace(int diceIndex1, int diceIndex2, int index1, int? index2) => Placed?.Invoke(this, diceIndex1, diceIndex2, index1, index2);
    }
}
