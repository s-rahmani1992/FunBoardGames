
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
        void RequestMoreCards();
        void VoteCard(bool positive);

        event Action<IEnumerable<CardData>> NewCardsReceived;
        event Action<ISETPlayer> PlayerStartedGuess;
        event Action<ISETPlayer> PlayerGuessTimeout;
        event Action<ISETPlayer, IEnumerable<CardData>, bool> PlayerGuessReceived;
        event Action<ISETPlayer> PlayerRequestCards;
        event Action<ISETPlayer, bool> PlayerVoteReceived;
        event Action<bool> VoteResultReceived;
        event Action<IEnumerable<ISETPlayer>> GameEnded;

        IEnumerable<ISETPlayer> Players { get; }
    }
}