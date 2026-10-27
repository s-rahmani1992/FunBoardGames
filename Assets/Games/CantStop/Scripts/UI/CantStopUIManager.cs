using FunBoardGames.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class CantStopUIManager : MonoBehaviour
    {
        const int DefaultWinConeCount = 3;
        const string ChooseColumnMessage = "Pick one column for the marker";
        const string BustSaverUsedMessage = "Bust Saver used. That roll doesn't count.";

        [SerializeField] UserGameHolder userGameHolder;

        [SerializeField] CantStopPlayerUI[] playerUis;
        [SerializeField] CantStopAssetManager playerColors;
        [SerializeField] Button rollButton;
        [SerializeField] Button playButton;
        [SerializeField] Button resetRoundButton;
        [SerializeField] Button bustSaverButton;
        [SerializeField] DiceController diceController;
        [SerializeField] GameBoardUI boardController;
        [SerializeField] WhiteConePanel whiteConePanel;
        [SerializeField] TextMeshProUGUI statText;
        [SerializeField] CantStopResultDialog resultDialog;

        Dictionary<ICantStopPlayer, CantStopPlayerUI> playerUiDict = new();

        SortedDictionary<int, (int, MarkMode)> possibleMoves = new();
        bool mustSelectMoves;

        CantStopGameData game;
        CantStopBoardData boardData;
        IEnumerable<int> selectedNumbers = new HashSet<int>();

        ICantStopGameHandler gameHandler;
        ICantStopPlayer currentPlayer;
        ICantStopPlayer selfPlayer;
        bool isbusted;
        bool isFirstRoll = true;
        bool resetRoundUsed = false;
        bool bustSaverUsed = false;

        // A round timer that started for a player whose turn has not begun on this screen yet, e.g. while the last round is still being shown
        (ICantStopPlayer player, DateTimeOffset startTime)? pendingTimer;

        // After a saved bust the round is played with the markers that are already placed, so the next Play ends the round without a pair
        bool playsSavedBust;

        private void Awake()
        {
            gameHandler = userGameHolder.GetGameHandler<ICantStopGameHandler>();

            rollButton.interactable = false;
            playButton.interactable = false;

            gameHandler.SignalGameLoaded();

            diceController.PairSelected += (v1, v2) =>
            {
                if (v1 != null)
                {
                    UpdatePossibleMoves(v1.Value, v2.Value);
                    bool allWrong = possibleMoves.All(m => m.Value.Item2 == MarkMode.Wrong);

                    if (mustSelectMoves && !allWrong)
                        statText.text = ChooseColumnMessage;
                    else
                        ClearChooseColumnMessage();

                    rollButton.interactable = !isbusted && !mustSelectMoves && !allWrong;
                    playButton.interactable =  isbusted || (!allWrong && !mustSelectMoves);
                    boardController.PreviewColumn(v1.Value, possibleMoves[v1.Value].Item1, possibleMoves[v1.Value].Item2);
                    boardController.PreviewColumn(v2.Value, possibleMoves[v2.Value].Item1, possibleMoves[v2.Value].Item2);
                }
                else
                {
                    playButton.interactable = isbusted;
                    ClearChooseColumnMessage();
                    boardController.ClearMarks();
                }
            };

            Subscribe();
        }

        private void OnGameReceived(ICantStopPlayer startPlayer)
        {
            game = userGameHolder.ActiveGame as CantStopGameData;
            boardData = game.BoardData;
            boardController.Initialize(boardData);
            int index = 0;
            int winConeCount = (userGameHolder.ActiveGame as CantStopGameData)?.WinConeCount ?? DefaultWinConeCount;

            foreach (var player in gameHandler.Players)
            {
                playerUis[index].SetPlayer(player, winConeCount);
                playerUiDict[player] = playerUis[index];

                if(player.IsMe)
                    selfPlayer = player;

                index++;
            }

            currentPlayer = startPlayer;
            playerUiDict[startPlayer].ToggleTurn(true);
            OnTurnStarted(startPlayer);
        }

        // The message is only shown while a choice is pending, so other messages are never wiped
        void ClearChooseColumnMessage()
        {
            if (statText.text == ChooseColumnMessage)
                statText.text = "";
        }

        void UpdatePossibleMoves(int v1, int v2)
        {
            possibleMoves.Clear();
            mustSelectMoves = false;

            if (v1 == v2)
            {
                var move = GetPosibleMove(v1);

                if(move == null || move.Value.pos >= boardData[v1] - 1)
                {
                    possibleMoves[v1] = (-1, MarkMode.Wrong);
                    return;
                }

                possibleMoves[v1] = (move.Value.pos + 1, MarkMode.Correct);
                return;
            }

            (int pos, int cone)? c1 = GetPosibleMove(v1);
            (int pos, int cone)? c2 = GetPosibleMove(v2);

            if (c1 == null && c2 == null) {
                possibleMoves[v1] = (-1, MarkMode.Wrong);
                possibleMoves[v2] = (-1, MarkMode.Wrong);
                return; 
            }
            int newWhiteCones = (c1 != null ? c1.Value.cone : 0) + (c2 != null ? c2.Value.cone : 0);

            mustSelectMoves = newWhiteCones + gameHandler.WhiteConePositions.Count() > game.MarkerConeCount;

            if (c1 == null)
                possibleMoves[v1] = (-1, MarkMode.Wrong);
            else
                possibleMoves[v1] = (c1.Value.pos, (mustSelectMoves ? MarkMode.Select : MarkMode.Correct));
            
            if (c2 == null)
                possibleMoves[v2] = (-1, MarkMode.Wrong);
            else
                possibleMoves[v2] = (c2.Value.pos, (mustSelectMoves ? MarkMode.Select : MarkMode.Correct));
        }

        (int pos, int cone)? GetPosibleMove(int columnNumber)
        {
            // A dormant column is not part of the game, so no marker can ever move there
            if (!boardData.HasColumn(columnNumber))
                return null;

            if (gameHandler.FinishedColumns.Contains(columnNumber))
                return null;

            int whiteConePos = -1;
            int newCone = 0;

            if (gameHandler.WhiteConePositions.TryGetValue(columnNumber, out whiteConePos) == false) // new white cone required
            {
                if (gameHandler.WhiteConePositions.Count() >= game.MarkerConeCount) // Check if we run out of white cones
                    return null;

                newCone = 1;
                if(selfPlayer.ConePositions.TryGetValue(columnNumber, out whiteConePos) == false)
                    whiteConePos = -1;
            }

            if (whiteConePos == -1)
                return (0, 1);

            if (whiteConePos >= boardData[columnNumber] - 1) // the cone is at 1 cell to the top
                return null;

            return (whiteConePos + 1, newCone);
        }

        private void Start()
        {
            rollButton.onClick.AddListener(OnRollClicked);
            playButton.onClick.AddListener(OnPlayClicked);
            resetRoundButton.onClick.AddListener(OnResetRoundClicked);
            bustSaverButton.onClick.AddListener(OnBustSaverClicked);
            boardController.SelectedChanged += OnSelectedChanged;
        }

        private void OnPlayClicked()
        {
            var g = diceController.SelectedIndices.ToArray();

            playButton.interactable = false;
            playsSavedBust = false;
            playerUiDict[currentPlayer].StopTimer();
            ClearChooseColumnMessage();
            if(isbusted || playsSavedBust)
                 gameHandler.EndRound();
            else
                gameHandler.PlayRound(diceController.SelectedIndices.ElementAtOrDefault(0), diceController.SelectedIndices.ElementAtOrDefault(1), selectedNumbers.Count() == 0 ? null : selectedNumbers.ElementAt(0));

            diceController.Block(true);
            diceController.Reset();
            boardController.ClearMarks();
        }

        private void OnSelectedChanged(IEnumerable<GameBoardColumn> columns)
        {
            selectedNumbers = columns.Select(c => c.Number);

            if (columns.Count() == 1)
            {
                rollButton.interactable = true;
                playButton.interactable = true;
                return;
            }
        }

        void Subscribe()
        {
            Unsubscribe();
            gameHandler.GameDataReceived += OnGameReceived;
            gameHandler.DiceRolled += OnRollChanged;
            gameHandler.WhiteConesPlaced += OnWhiteConePlaced;
            gameHandler.RoundPlayed += OnRoundPlayed;
            gameHandler.RoundCanceled += OnRoundCanceled;
            gameHandler.ResetRoundReceived += OnResetRoundReceived;
            gameHandler.BustSaverReceived += OnBustSaverReceived;
            gameHandler.TimedOut += OnTimedOut;
            gameHandler.RoundStarted += OnRoundStarted;
            gameHandler.GameFinished += OnGameFinished;
        }

        private void OnRoundStarted(DateTimeOffset startTime, ICantStopPlayer player)
        {
            player ??= currentPlayer;

            if (player == currentPlayer)
                StartTurnTimer(player, startTime);
            else
                pendingTimer = (player, startTime); // started when that player's turn begins on this screen
        }

        void StartTurnTimer(ICantStopPlayer player, DateTimeOffset startTime)
        {
            pendingTimer = null;
            float elapsed = (float)(DateTimeOffset.UtcNow - startTime).TotalSeconds;
            playerUiDict[player].StartTimer(game.RoundTime, elapsed);
        }

        private async void OnTimedOut(ICantStopPlayer player, ICantStopPlayer nextPlayer)
        {
            statText.text = (player.IsMe ? "You" : player.Profile.PlayerName) + " ran out of time!";
            whiteConePanel.UpdateUI(game.MarkerConeCount);
            boardController.RemoveWhiteCones();
            OnTurnStarted(nextPlayer);
            await System.Threading.Tasks.Task.Delay(2000);
            statText.text = "";
        }

        private void OnResetRoundReceived()
        {
            if(currentPlayer.IsMe == false)
            {
                statText.text = (currentPlayer.IsMe ? "You" : currentPlayer.Profile.PlayerName) + " Reset the Round!";
                diceController.ClearSelection();
                diceController.Reset();
                boardController.RemoveWhiteCones();
                boardController.ClearMarks();
                whiteConePanel.UpdateUI(game.MarkerConeCount);
            }
            else
                rollButton.interactable = true;

            isFirstRoll = true;
        }

        private async void OnGameFinished(IEnumerable<ICantStopPlayer> players)
        {
            await System.Threading.Tasks.Task.Delay(2500);
            DialogManager.Instance.ShowDialog(resultDialog, DialogShowOptions.OverAll, players);
        }

        private async void OnRoundCanceled(ICantStopPlayer player1, ICantStopPlayer nextPlayer)
        {
            statText.text = (player1.IsMe ? "You" : player1.Profile.PlayerName) + " Canceled the Round!";
            whiteConePanel.UpdateUI(game.MarkerConeCount);
            boardController.RemoveWhiteCones();
            OnTurnStarted(nextPlayer);
            await System.Threading.Tasks.Task.Delay(2000);
            statText.text = "";
        }

        private async void OnWhiteConePlaced(ICantStopPlayer player, IDictionary<int, int> dictionary, int dice1, int dice2)
        {
            foreach (var p in dictionary)
                boardController.PlaceCone(p.Key, PlayerColor.None, p.Value);

            whiteConePanel.UpdateUI(game.MarkerConeCount - gameHandler.WhiteConePositions.Count());

            if (player != selfPlayer)
            {
                diceController.PickDices(dice1, dice2);
                await System.Threading.Tasks.Task.Delay(2000);
                diceController.ClearSelection();
            }
            else
                rollButton.interactable = true;
        }

        private async void OnRoundPlayed(ICantStopPlayer player, IDictionary<int, int> updatedWhitecones, int dice1, int dice2, ICantStopPlayer nextPlayer)
        {
            foreach (var p in updatedWhitecones)
                boardController.PlaceCone(p.Key, PlayerColor.None, p.Value);

            whiteConePanel.UpdateUI(game.MarkerConeCount - gameHandler.WhiteConePositions.Count());

            if (player != selfPlayer)
            {
                if (dice1 >= 0 && dice2 >= 0) // no pair was placed when the round is played after a saved bust
                    diceController.PickDices(dice1, dice2);

                await System.Threading.Tasks.Task.Delay(2000);
                diceController.ClearSelection();

                foreach(var p in currentPlayer.ConePositions)
                {
                    boardController.PlaceCone(p.Key, currentPlayer.ConeColor, p.Value);
                }

                boardController.RemoveWhiteCones();
            }
            else
            {
                await System.Threading.Tasks.Task.Delay(2000);
                foreach (var p in currentPlayer.ConePositions)
                {
                    boardController.PlaceCone(p.Key, currentPlayer.ConeColor, p.Value);
                }
                boardController.RemoveWhiteCones();
            }

            playerUiDict[player].UpdateScore(player.Score);

            OnTurnStarted(nextPlayer);
        }

        void Unsubscribe()
        {
            gameHandler.GameDataReceived -= OnGameReceived;
            gameHandler.DiceRolled -= OnRollChanged;
            gameHandler.WhiteConesPlaced -= OnWhiteConePlaced;
            gameHandler.RoundPlayed -= OnRoundPlayed;
            gameHandler.RoundCanceled -= OnRoundCanceled;
            gameHandler.ResetRoundReceived -= OnResetRoundReceived;
            gameHandler.BustSaverReceived -= OnBustSaverReceived;
            gameHandler.TimedOut -= OnTimedOut;
            gameHandler.RoundStarted -= OnRoundStarted;
            gameHandler.GameFinished -= OnGameFinished;
        }

        private void OnTurnStarted(ICantStopPlayer player)
        {
            if(currentPlayer != null)
            {
                playerUiDict[currentPlayer].ToggleTurn(false);
            }

            foreach (var playerUi in playerUiDict.Values)
                playerUi.HideTimer();

            isFirstRoll = true;
            resetRoundUsed = false;
            bustSaverUsed = false;
            playsSavedBust = false;
            currentPlayer = player;
            playerUiDict[player].ToggleTurn(true);
            rollButton.interactable = player.IsMe;
            playButton.interactable = false;
            resetRoundButton.interactable = false;
            bustSaverButton.interactable = false;
            whiteConePanel.UpdateUI(game.MarkerConeCount);
            diceController.Reset();
            boardController.RemoveWhiteCones();
            boardController.ClearMarks();

            if (pendingTimer != null && pendingTimer.Value.player == player)
                StartTurnTimer(player, pendingTimer.Value.startTime);
        }

        private void OnRollChanged(int[] dices, bool isBusted)
        {
            diceController.Block(currentPlayer.IsMe == false);
            diceController.SetDiceValues(dices);
            this.isbusted = isBusted;
            playButton.interactable = currentPlayer.IsMe && isbusted;
            rollButton.interactable = false;
            resetRoundButton.interactable = currentPlayer.IsMe && !resetRoundUsed && !isbusted;
            bustSaverButton.interactable = currentPlayer.IsMe && !bustSaverUsed && isbusted;
            if (isbusted)
                statText.text = (currentPlayer.IsMe ? "You are Busted!" : $"{currentPlayer.Profile.PlayerName} is Busted!");
        }

        void OnRollClicked()
        {
            playsSavedBust = false;
            playerUiDict[currentPlayer].StopTimer();
            rollButton.interactable = false;
            diceController.StartRoll();

            if(isFirstRoll)
            {
                isFirstRoll = false;
                gameHandler.RollDice();
            }
            else
            {
                playButton.interactable = false;
                ClearChooseColumnMessage();
                gameHandler.PlaceWhiteCones(diceController.SelectedIndices.ElementAtOrDefault(0), diceController.SelectedIndices.ElementAtOrDefault(1), selectedNumbers.Count() == 0 ? null : selectedNumbers.ElementAt(0));

                diceController.Block(true);
                diceController.Reset();
                boardController.ClearMarks();
            }
        }

        void OnBustSaverClicked()
        {
            if (bustSaverUsed || currentPlayer.IsMe == false || !isbusted)
                return;

            bustSaverUsed = true;
            bustSaverButton.interactable = false;
            playButton.interactable = false;

            // The busted roll does not count: the dice go back to unrolled and the player waits for the server's answer
            isbusted = false;
            diceController.ClearSelection();
            diceController.Reset();
            boardController.ClearMarks();
            gameHandler.UseBustSaver();
        }

        private async void OnBustSaverReceived()
        {
            isbusted = false;
            bustSaverButton.interactable = false;

            if (currentPlayer.IsMe)
            {
                // The busted roll does not count, so the screen is back to just before it: the markers stay where they were
                // placed and the player is about to roll again, or to play them
                statText.text = BustSaverUsedMessage;
                isFirstRoll = true; // the next roll places nothing, so it is a plain roll
                playsSavedBust = true;
                rollButton.interactable = true;
                playButton.interactable = gameHandler.WhiteConePositions.Any();
            }
            else
            {
                diceController.ClearSelection();
                diceController.Reset();
                boardController.ClearMarks();
                statText.text = currentPlayer.Profile.PlayerName + " used Bust Saver!";
            }

            string shown = statText.text;
            await System.Threading.Tasks.Task.Delay(2500);

            if (statText != null && statText.text == shown)
                statText.text = "";
        }

        void OnResetRoundClicked()
        {
            if(resetRoundUsed || currentPlayer.IsMe == false || isbusted)
                return;

            resetRoundButton.interactable = false;
            playButton.interactable = false;
            gameHandler.ResetRound();
            diceController.ClearSelection();
            diceController.Reset();
            boardController.RemoveWhiteCones();
            boardController.ClearMarks();
            whiteConePanel.UpdateUI(game.MarkerConeCount);
            resetRoundUsed = true;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
