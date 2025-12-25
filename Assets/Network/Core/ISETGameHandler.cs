
using FunBoardGames.SET;
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface ISETGameHandler: IGameHandler
    {
        void SignalGameLoaded();

        event Action<IEnumerable<CardData>> NewCardsReceived;

        IEnumerable<ISETPlayer> Players { get; }
    }
}