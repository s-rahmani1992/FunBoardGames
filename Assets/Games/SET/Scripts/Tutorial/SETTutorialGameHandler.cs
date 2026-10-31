using DG.Tweening;
using FunBoardGames.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FunBoardGames.SET
{
    public class SETTutorialGameHandler : ISETGameHandler, IDisposable
    {
        public const int StartCardCount = 12;
        public const int WrongLimit = 3;
        public const int HintLimit = 1;

        static readonly CardData[] StartingTable =
        {
            new(1, 2, 0, 1), new(0, 2, 0, 1), new(0, 2, 0, 2), new(2, 0, 2, 2),
            new(1, 0, 1, 0), new(2, 1, 0, 0), new(2, 0, 2, 0), new(2, 2, 2, 0),
            new(0, 0, 0, 0), new(0, 0, 1, 1), new(2, 2, 2, 2), new(0, 1, 2, 2),
        };

        static readonly CardData[] ReserveCards =
        {
            new(2, 2, 1, 2), new(0, 2, 1, 1), new(2, 1, 0, 2),
        };

        public event Action GameStarted;
        public event Action<IEnumerable<CardData>> NewCardsReceived;
        public event Action<ISETPlayer, DateTimeOffset> PlayerStartedGuess;
        public event Action<ISETPlayer, IEnumerable<CardData>, bool> PlayerGuessReceived;
        public event Action<DateTimeOffset> RoundStarted;
        public event Action<ISETPlayer> PlayerUsedHint;
        public event Action<CardData> CardHintReceived;

#pragma warning disable CS0067 // There are no other players, no timeouts and no game end in the tutorial, the events only satisfy the interface
        public event Action<IBoardGamePlayer> PlayerJoined;
        public event Action<IBoardGamePlayer> PlayerLeft;
        public event Action AllPlayersReady;
        public event Action<ISETPlayer> PlayerGuessTimeout;
        public event Action<ISETPlayer> PlayerBusted;
        public event Action<IEnumerable<ISETPlayer>> GameEnded;
        public event Action<IEnumerable<CardData>> RoundTimedOut;
#pragma warning restore CS0067

        readonly TutorialSETPlayer player;
        readonly List<CardData> table = new();
        readonly Queue<CardData> reserve = new(ReserveCards);
        Tween startTween;
        bool isGuessing;

        public IEnumerable<ISETPlayer> Players => new ISETPlayer[] { player };

        public ISETPlayer Me => player;

        public static SETGameData CreateGameData()
        {
            return new SETGameData(0, 7f, "Tutorial", 1, 4, 0, WrongLimit, HintLimit, StartCardCount);
        }

        public SETTutorialGameHandler(ProfileData profile)
        {
            player = new TutorialSETPlayer(profile);
        }

        public IReadOnlyList<CardData> FindSet()
        {
            for (int i = 0; i < table.Count - 2; i++)
            {
                for (int j = i + 1; j < table.Count - 1; j++)
                {
                    for (int k = j + 1; k < table.Count; k++)
                    {
                        if (CardData.IsSET(CardData.CheckSET(table[i], table[j], table[k])))
                            return new[] { table[i], table[j], table[k] };
                    }
                }
            }

            return null;
        }

        public void SignalGameLoaded()
        {
            startTween?.Kill();
            startTween = DOVirtual.DelayedCall(0.2f, BeginGame);
        }

        void BeginGame()
        {
            table.AddRange(StartingTable);
            NewCardsReceived?.Invoke(StartingTable.ToList());
            GameStarted?.Invoke();
            RoundStarted?.Invoke(DateTimeOffset.UtcNow);
        }

        public void StartGuess()
        {
            if (isGuessing)
                return;

            isGuessing = true;
            PlayerStartedGuess?.Invoke(player, DateTimeOffset.UtcNow);
        }

        public void GuessCards(IEnumerable<CardData> cards)
        {
            var guessed = cards.ToList();
            isGuessing = false;

            bool isCorrect = guessed.Count == 3 && CardData.IsSET(CardData.CheckSET(guessed[0], guessed[1], guessed[2]));

            if (isCorrect)
            {
                player.CorrectScore++;
                var newCards = ReplaceCards(guessed);

                if (newCards.Count > 0)
                    NewCardsReceived?.Invoke(newCards);
            }
            else
            {
                player.WrongScore = Math.Min(player.WrongScore + 1, WrongLimit - 1);
            }

            PlayerGuessReceived?.Invoke(player, guessed, isCorrect);

            if (isCorrect)
                RoundStarted?.Invoke(DateTimeOffset.UtcNow);
        }

        List<CardData> ReplaceCards(List<CardData> removed)
        {
            foreach (var card in removed)
                table.RemoveAll(c => c.Equals(card));

            var newCards = new List<CardData>();

            while (newCards.Count < removed.Count && reserve.Count > 0)
                newCards.Add(reserve.Dequeue());

            table.AddRange(newCards);
            return newCards;
        }

        public void RequestCardHint()
        {
            var set = FindSet();

            if (set == null || player.UsedHintCount >= HintLimit)
                return;

            player.UsedHintCount++;
            PlayerUsedHint?.Invoke(player);
            CardHintReceived?.Invoke(set[0]);
        }

        public void LeaveGame()
        {
        }

        public void ReadyUp()
        {
        }

        public void Dispose()
        {
            startTween?.Kill();
        }
    }
}
