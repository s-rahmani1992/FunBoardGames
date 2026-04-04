
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface ICantStopGameHandler : IGameHandler
    {
        event Action<CantStopBoardData, ICantStopPlayer> GameDataReceived;
        event Action<int[]> DiceRolled;
        event Action<ICantStopPlayer, IDictionary<int, int>, int, int> WhiteConesPlaced;
        void SignalGameLoaded();
        void RollDice();
        void PlaceWhiteCones(int diceIndex1, int diceIndex2, int? columnIndex1);

        IEnumerable<ICantStopPlayer> Players { get; }

        IDictionary<int, int> WhiteConePositions { get; }

        IEnumerable<int> FinishedColumns { get; }
    }
}
