
using FunBoardGames.SET;
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface ISETGameHandler: IGameHandler
    {
        void SignalGameLoaded();
        void StartGuess();
        void GuessCards(IEnumerable<CardData> cards);

        event Action GameStarted;
        event Action<IEnumerable<CardData>> NewCardsReceived;
        event Action<ISETPlayer, DateTimeOffset> PlayerStartedGuess;
        event Action<ISETPlayer> PlayerGuessTimeout;
        event Action<ISETPlayer, IEnumerable<CardData>, bool> PlayerGuessReceived;
        event Action<IEnumerable<ISETPlayer>> GameEnded;

        IEnumerable<ISETPlayer> Players { get; }
    }
}