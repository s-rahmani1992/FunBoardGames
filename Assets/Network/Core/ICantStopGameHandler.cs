
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface ICantStopGameHandler : IGameHandler
    {
        event Action<CantStopBoardData, ICantStopPlayer> GameDataReceived;
        event Action<int[]> DiceRolled;

        void SignalGameLoaded();
        void RollDice();

        IEnumerable<ICantStopPlayer> Players { get; }
    }
}
