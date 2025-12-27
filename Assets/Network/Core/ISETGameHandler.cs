
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

        event Action<IEnumerable<CardData>> NewCardsReceived;
        event Action<ISETPlayer> PlayerStartedGuess;
        event Action<ISETPlayer> PlayerGuessTimeout;
        event Action<ISETPlayer, IEnumerable<CardData>, bool> PlayerGuessReceived;

        IEnumerable<ISETPlayer> Players { get; }
    }
}