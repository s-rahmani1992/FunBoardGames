
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
        void RequestCardHint();

        event Action GameStarted;
        event Action<IEnumerable<CardData>> NewCardsReceived;
        event Action<ISETPlayer, DateTimeOffset> PlayerStartedGuess;
        event Action<ISETPlayer> PlayerGuessTimeout;
        event Action<ISETPlayer> PlayerBusted;
        event Action<ISETPlayer, IEnumerable<CardData>, bool> PlayerGuessReceived;
        event Action<IEnumerable<ISETPlayer>> GameEnded;
        event Action<DateTimeOffset> RoundStarted;
        event Action<IEnumerable<CardData>> RoundTimedOut;
        event Action<ISETPlayer> PlayerUsedHint;
        event Action<CardData> CardHintReceived;

        IEnumerable<ISETPlayer> Players { get; }
    }
}