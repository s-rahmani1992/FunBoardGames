
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface ICantStopGameHandler : IGameHandler
    {
        event Action<CantStopBoardData, ICantStopPlayer> GameDataReceived;
        event Action<int[], bool> DiceRolled;
        event Action<ICantStopPlayer, IDictionary<int, int>, int, int> WhiteConesPlaced;
        event Action<ICantStopPlayer, IDictionary<int, int>, int, int, ICantStopPlayer> RoundPlayed;
        event Action<ICantStopPlayer, ICantStopPlayer> RoundCanceled;
        void SignalGameLoaded();
        void RollDice();
        void PlaceWhiteCones(int diceIndex1, int diceIndex2, int? columnIndex1);
        void PlayRound(int diceIndex1, int diceIndex2, int? columnIndex1);
        void CancelRound();

        IEnumerable<ICantStopPlayer> Players { get; }

        IDictionary<int, int> WhiteConePositions { get; }

        IEnumerable<int> FinishedColumns { get; }
    }
}
