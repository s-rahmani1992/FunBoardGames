
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface ICantStopGameHandler : IGameHandler
    {
        event Action<CantStopBoardData, ICantStopPlayer> GameDataReceived;

        void SignalGameLoaded();

        IEnumerable<ICantStopPlayer> Players { get; }
    }
}
