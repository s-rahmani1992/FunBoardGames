using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FunBoardGames.Network;
using DG.Tweening;
using System.Linq;
using System;

namespace FunBoardGames.SET
{
    public class SETGameUIManager : MonoBehaviour
    {
        [SerializeField] CardDeckManager cardDeckManager;
        [SerializeField] ObjectPoolManager pool;
        [SerializeField] Button guessBtn, hintBtn;
        [SerializeField] GameLogger gameLogger;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] PlayerUI[] playerUIs;
        [SerializeField] SETVoteDialog voteDialog;
        [SerializeField] SETResultDialog resultDialog;
        [SerializeField] Timer timer;
        [SerializeField] ProgressTimer roundTimer;

        Dictionary<ISETPlayer, PlayerUI> playerUIMap = new();

        SETGameData gameData;

        public static SETGameUIManager Instance { get; private set; }

        public Color[] colors;
        public Sprite[] cardShapes;

        List<ISETPlayer> players = new();
        List<CardUI> hints = new(3);
        List<CardUI> placedCardUIs = new(18);

        ISETGameHandler setGameHandler;
        ISETPlayer selfPlayer;
        List<ISETPlayer> playerResult;
        bool gameFinished = false;
        bool isWaitingForCardDestribution = false;
        bool hasUsedHintThisRound = false;

        // Start time of a round that begins once the cards on the table are settled
        DateTimeOffset? pendingRoundStartTime = null;

        private void Awake()
        {
            Instance = this;
            gameData = userGameHolder.ActiveGame as SETGameData;
            setGameHandler = userGameHolder.GetGameHandler<ISETGameHandler>();
            cardDeckManager.RegisterGameHandler(setGameHandler, gameData);
            setGameHandler.SignalGameLoaded();
            roundTimer.Clear();
            Subscribe();
            UpdatePlayerUIs();
        }

        void BeginGameCountDown()
        {
            timer.StartCountdown(4);
            gameLogger.SetText("Wait For The Game To Start.");
            WaitForCardDestribution();
            DOVirtual.DelayedCall(4, () =>
            {
                gameLogger.SetText("Destributing Cards");
                RefreshBtns(SETGameState.Destribute);
                cardDeckManager.DestributePendingCards();
            });
        }

        void Subscribe()
        {
            setGameHandler.GameStarted += OnGameStarted;
            setGameHandler.PlayerStartedGuess += OnPlayerStartedGuess;
            setGameHandler.PlayerGuessTimeout += OnPlayerGuessTimeout;
            setGameHandler.PlayerBusted += OnPlayerBusted;
            setGameHandler.PlayerGuessReceived += OnPlayerGuessReceived;
            setGameHandler.GameEnded += OnGameFinished;
            setGameHandler.RoundStarted += OnRoundStarted;
            setGameHandler.RoundTimedOut += OnRoundTimedOut;
            setGameHandler.PlayerUsedHint += OnPlayerUsedHint;
        }

        private void OnRoundStarted(DateTimeOffset roundStartTime)
        {
            pendingRoundStartTime = roundStartTime;
            hasUsedHintThisRound = false;

            foreach (var playerUI in playerUIMap.Values)
                playerUI.ResetRoundHint();
        }

        private void OnPlayerUsedHint(ISETPlayer player)
        {
            playerUIMap[player].UseHint();

            if (player.IsMe)
            {
                hasUsedHintThisRound = true;
                hintBtn.interactable = false;
            }
            else
                gameLogger.Toast($"{player.Name} used a hint!");
        }

        bool CanUseHint()
        {
            if (hasUsedHintThisRound || selfPlayer == null)
                return false;

            return gameData.HintLimit == null || selfPlayer.UsedHintCount < gameData.HintLimit;
        }

        private void OnRoundTimedOut(IEnumerable<CardData> removedCards)
        {
            roundTimer.Clear();
            gameLogger.Toast("Time's up! Nobody found the SET.");
            RefreshBtns(SETGameState.Destribute);
            WaitForCardDestribution();
        }

        void StartPendingRound()
        {
            if (pendingRoundStartTime == null)
                return;

            float elapsed = (float)(DateTimeOffset.UtcNow - pendingRoundStartTime.Value).TotalSeconds;
            roundTimer.StartTimer(gameData.RoundTime, elapsed);
            pendingRoundStartTime = null;
        }

        void WaitForCardDestribution()
        {
            // Unsubscribe first so overlapping flows, like a wrong guess followed by a round timeout, only subscribe once
            cardDeckManager.CardDestributionEnded -= OnCardDestributionEnded;
            cardDeckManager.CardDestributionEnded += OnCardDestributionEnded;
            isWaitingForCardDestribution = true;
        }

        private void OnGameStarted()
        {
            BeginGameCountDown();
        }

        private void OnGameFinished(IEnumerable<ISETPlayer> players)
        {
            gameFinished = true;
            roundTimer.Clear();
            playerResult = new(players);

            // Nothing is animating, e.g. the game ended by a guess timeout, so the results are shown right away
            if (isWaitingForCardDestribution == false)
                ShowGameResult();
        }

        private void OnPlayerBusted(ISETPlayer player)
        {
            playerUIMap[player].RefreshStatus();
            gameLogger.Toast(player.IsMe ? "You are busted! You can't guess anymore." : $"{player.Name} is busted!");

            if (player.IsMe)
                guessBtn.interactable = hintBtn.interactable = false;
        }

        private void OnPlayerGuessReceived(ISETPlayer player, IEnumerable<CardData> enumerable, bool isCorrect)
        {
            gameLogger.SetText("");
            timer.Stop();

            if (isCorrect)
                roundTimer.Clear();

            playerUIMap[player].UpdateScores(); 
            playerUIMap[player].ToggleGuess(false);
            StartCoroutine(DisplayResult(isCorrect, player));
            WaitForCardDestribution();
        }

        private void OnPlayerGuessTimeout(ISETPlayer player)
        {
            gameLogger.Toast(player.IsMe ? "You didn't guess in time. You lost 1 point" : $"{player.Name} didn't guess in time. {player.Name} lost 1 point");
            playerUIMap[player].ToggleGuess(false);
            playerUIMap[player].UpdateScores();
            RefreshBtns(SETGameState.Normal);
        }

        private void UpdatePlayerUIs()
        {
            players = new(setGameHandler.Players);

            int index = 0;

            foreach (var player in players)
            {
                playerUIs[index].SetPlayer(player, gameData.WrongLimit, gameData.HintLimit);
                player.LeftGame += () => players.Remove(player);
                playerUIMap.Add(player, playerUIs[index]);

                if (player.IsMe)
                    selfPlayer = player;

                index++;
            }
        }

        private void OnPlayerStartedGuess(ISETPlayer player, DateTimeOffset guessStartTime)
        {
            RefreshBtns(SETGameState.Guess);
            playerUIMap[player].ToggleGuess(true);
            timer.StartCountdown((float)(DateTimeOffset.UtcNow - guessStartTime).TotalSeconds + 7.0f); //TODO sync with game data in future
            gameLogger.SetText(player.IsMe ? "Your are guessing. Guess quickly" : $"{player.Name} is guessing");
        }

        void UnSubscribe()
        {
            setGameHandler.GameStarted -= OnGameStarted;
            setGameHandler.PlayerStartedGuess -= OnPlayerStartedGuess;
            setGameHandler.PlayerGuessTimeout -= OnPlayerGuessTimeout;
            setGameHandler.PlayerBusted -= OnPlayerBusted;
            setGameHandler.PlayerGuessReceived -= OnPlayerGuessReceived;
            setGameHandler.GameEnded -= OnGameFinished;
            setGameHandler.RoundStarted -= OnRoundStarted;
            setGameHandler.RoundTimedOut -= OnRoundTimedOut;
            setGameHandler.PlayerUsedHint -= OnPlayerUsedHint;
        }

        private void OnDestroy()
        {
            UnSubscribe();
            Instance = null;
        }

        private void OnCardDestributionEnded()
        {
            cardDeckManager.CardDestributionEnded -= OnCardDestributionEnded;
            isWaitingForCardDestribution = false;

            if(gameFinished)
            {
                ShowGameResult();
                return;
            }

            gameLogger.SetText("");
            RefreshBtns(SETGameState.Normal);
            StartPendingRound();
        }

        void ShowGameResult()
        {
            gameLogger.SetText("Game Finished!");
            RefreshBtns(SETGameState.Finish);
            DialogManager.Instance.ShowDialog(resultDialog, DialogShowOptions.OverAll, playerResult.AsEnumerable());
        }

        void RefreshBtns(SETGameState state)
        {
            // A busted player can't guess for the rest of the game
            guessBtn.interactable = hintBtn.interactable = (state == SETGameState.Normal && selfPlayer?.IsBusted != true);
            hintBtn.interactable &= CanUseHint();
            for (int i = 0; i < hints.Count; i++)
                hints[i].Mark(false);
            hints.Clear();
        }

        public void AttemptGuess()
        {
            if (selfPlayer?.IsBusted == true)
                return;

            setGameHandler.StartGuess();
        }

        IEnumerator DisplayResult(bool isSet, ISETPlayer player)
        {
            yield return new WaitForSeconds(0.5f);

            if (isSet)
                gameLogger.SetText(player.IsMe ? "You Guessed Right! You Got 1 point." : $"{player.Name} Guessed Right! He Got 1 point.");
            else
                gameLogger.SetText(player.IsMe ? "Your Guess was Wrong! You lost 1 point." : $"{player.Name}'s Guess was Wrong! He lost 1 point.");

            yield return new WaitForSeconds(6);
            DialogManager.Instance.CloseDialog<GuessResultDialog>();
        }

        public void SendHint()
        {
            if (selfPlayer?.IsBusted == true || CanUseHint() == false)
                return;

            setGameHandler.RequestCardHint();
        }

        public void MarkHints(CardData[] cards)
        {
            hints.Clear();
            if(cards.Length == 0) return;
            foreach (var c in placedCardUIs)
            {
                if (c.info.Equals(cards[0]) || c.info.Equals(cards[1]) || c.info.Equals(cards[2]))
                {
                    c.MarkHint();
                    hints.Add(c);
                }
            }
        }
    }
}
