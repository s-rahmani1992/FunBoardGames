using DG.Tweening;
using FunBoardGames.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FunBoardGames.CantStop
{
    public class CantStopTutorialGameHandler : ICantStopGameHandler, IDisposable
    {
        public const int MarkerConeCount = 3;
        public const int WinConeCount = 3;

        const int NoDice = -1;
        const float DiceDelay = 0.6f;

        static readonly Dictionary<int, int> Columns = new()
        {
            { 2, 3 }, { 3, 5 }, { 4, 7 }, { 5, 9 }, { 6, 11 }, { 7, 13 },
            { 8, 11 }, { 9, 9 }, { 10, 7 }, { 11, 5 }, { 12, 3 },
        };

        /// <summary>The dice the tutorial hands out, in order. A roll and a placement both take the next set.</summary>
        public static readonly (int[] Dice, bool IsBusted)[] DiceSets =
        {
            (new[] { 1, 3, 2, 5 }, false), // turn 1: roll
            (new[] { 2, 3, 4, 6 }, false), // turn 1: roll again
            (new[] { 4, 1, 3, 6 }, false), // turn 2: roll
            (new[] { 1, 1, 2, 1 }, false), // turn 2: a roll worth resetting
            (new[] { 3, 4, 2, 5 }, false), // turn 2: roll after the reset
            (new[] { 1, 2, 5, 6 }, false), // turn 3: roll
            (new[] { 5, 6, 1, 3 }, false), // turn 3: roll again
            (new[] { 2, 6, 6, 6 }, true),  // turn 3: busted
            (new[] { 1, 3, 4, 2 }, false), // turn 3: roll after the bust saver
        };

        public event Action<ICantStopPlayer> GameDataReceived;
        public event Action<int[], bool> DiceRolled;
        public event Action<ICantStopPlayer, IDictionary<int, int>, int, int> WhiteConesPlaced;
        public event Action<ICantStopPlayer, IDictionary<int, int>, int, int, ICantStopPlayer> RoundPlayed;
        public event Action<ICantStopPlayer, ICantStopPlayer> RoundCanceled;
        public event Action ResetRoundReceived;
        public event Action BustSaverReceived;

#pragma warning disable CS0067 // There are no other players, no timeouts and no game end in the tutorial, the events only satisfy the interface
        public event Action<IBoardGamePlayer> PlayerJoined;
        public event Action<IBoardGamePlayer> PlayerLeft;
        public event Action AllPlayersReady;
        public event Action<IEnumerable<ICantStopPlayer>> GameFinished;
        public event Action<ICantStopPlayer, ICantStopPlayer> TimedOut;
        public event Action<DateTimeOffset, ICantStopPlayer> RoundStarted;
#pragma warning restore CS0067

        readonly TutorialCantStopPlayer player;
        readonly SortedDictionary<int, int> whiteCones = new();
        readonly HashSet<int> finishedColumns = new();
        int[] dice = new int[4];
        bool isBusted;
        Tween pendingTween;

        public IEnumerable<ICantStopPlayer> Players => new ICantStopPlayer[] { player };

        public IDictionary<int, int> WhiteConePositions => whiteCones;

        public IEnumerable<int> FinishedColumns => finishedColumns;

        public int DiceSetsDelivered { get; private set; }
        public int RoundsPlayed { get; private set; }
        public int ResetCount { get; private set; }
        public int BustSaverCount { get; private set; }

        public static CantStopGame CreateGameData()
        {
            return new CantStopGame()
            {
                Id = 0,
                Name = "Tutorial",
                Type = BoardGame.CantStop,
                NumberofPlayers = 1,
                MarkerConeCount = MarkerConeCount,
                WinConeCount = WinConeCount,
                BoardData = new CantStopBoardData(Columns),
                RoundTime = 0,
                TimeoutLimit = 3,
            };
        }

        public CantStopTutorialGameHandler(ProfileData profile)
        {
            player = new TutorialCantStopPlayer(profile);
        }

        public void SignalGameLoaded()
        {
            pendingTween?.Kill();
            pendingTween = DOVirtual.DelayedCall(0.2f, () => GameDataReceived?.Invoke(player));
        }

        public void RollDice()
        {
            DeliverDice(null);
        }

        public void PlaceWhiteCones(int diceIndex1, int diceIndex2, int? columnIndex1)
        {
            var updated = ApplyPick(diceIndex1, diceIndex2, columnIndex1);
            DeliverDice(() => WhiteConesPlaced?.Invoke(player, updated, diceIndex1, diceIndex2));
        }

        public void PlayRound(int diceIndex1, int diceIndex2, int? columnIndex1)
        {
            var updated = ApplyPick(diceIndex1, diceIndex2, columnIndex1);
            CommitCones();
            RoundsPlayed++;
            RoundPlayed?.Invoke(player, updated, diceIndex1, diceIndex2, player);
            whiteCones.Clear();
        }

        public void EndRound()
        {
            if (isBusted)
            {
                isBusted = false;
                whiteCones.Clear();
                RoundsPlayed++;
                RoundCanceled?.Invoke(player, player);
                return;
            }

            // The round is played after a saved bust, so there is no pair to place
            CommitCones();
            RoundsPlayed++;
            RoundPlayed?.Invoke(player, new SortedDictionary<int, int>(), NoDice, NoDice, player);
            whiteCones.Clear();
        }

        public void ResetRound()
        {
            whiteCones.Clear();
            ResetCount++;
            ResetRoundReceived?.Invoke();
        }

        public void UseBustSaver()
        {
            isBusted = false;
            BustSaverCount++;
            BustSaverReceived?.Invoke();
        }

        public void LeaveGame()
        {
        }

        public void ReadyUp()
        {
        }

        public void Dispose()
        {
            pendingTween?.Kill();
        }

        // The dice take a moment to arrive, like the answer of a server
        void DeliverDice(Action beforeDice)
        {
            pendingTween?.Kill();
            pendingTween = DOVirtual.DelayedCall(DiceDelay, () =>
            {
                beforeDice?.Invoke();

                var set = DiceSets[Math.Min(DiceSetsDelivered, DiceSets.Length - 1)];
                dice = (int[])set.Dice.Clone();
                isBusted = set.IsBusted;
                DiceSetsDelivered++;
                DiceRolled?.Invoke((int[])dice.Clone(), isBusted);
            });
        }

        SortedDictionary<int, int> ApplyPick(int diceIndex1, int diceIndex2, int? chosenColumn)
        {
            var rest = Enumerable.Range(0, dice.Length).Where(i => i != diceIndex1 && i != diceIndex2).ToArray();
            int first = dice[diceIndex1] + dice[diceIndex2];
            int second = dice[rest[0]] + dice[rest[1]];
            var updated = new SortedDictionary<int, int>();

            if (first == second)
            {
                if (CanAdvance(first))
                    Advance(first, 2, updated);

                return updated;
            }

            var moves = new[] { first, second }.Where(CanAdvance).ToList();
            int newCones = moves.Count(column => whiteCones.ContainsKey(column) == false);

            // Not enough markers for both columns, so the player chose one
            if (whiteCones.Count + newCones > MarkerConeCount && moves.Count > 0)
                moves = new List<int> { chosenColumn.HasValue && moves.Contains(chosenColumn.Value) ? chosenColumn.Value : moves[0] };

            foreach (var column in moves)
                Advance(column, 1, updated);

            return updated;
        }

        int PositionOf(int column)
        {
            if (whiteCones.TryGetValue(column, out int white))
                return white;

            return player.ConePositions.TryGetValue(column, out int own) ? own : -1;
        }

        bool CanAdvance(int column)
        {
            if (Columns.TryGetValue(column, out int height) == false || finishedColumns.Contains(column))
                return false;

            if (whiteCones.ContainsKey(column) == false && whiteCones.Count >= MarkerConeCount)
                return false;

            return PositionOf(column) < height - 1;
        }

        void Advance(int column, int steps, IDictionary<int, int> updated)
        {
            int position = Math.Min(PositionOf(column) + steps, Columns[column] - 1);
            whiteCones[column] = position;
            updated[column] = position;
        }

        void CommitCones()
        {
            foreach (var cone in whiteCones)
            {
                player.ConePositions[cone.Key] = cone.Value;

                if (cone.Value >= Columns[cone.Key] - 1 && finishedColumns.Add(cone.Key))
                    player.Score++;
            }
        }
    }
}
