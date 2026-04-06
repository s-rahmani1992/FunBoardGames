using FunBoardGames.Network;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class CantStopUIManager : MonoBehaviour
    {
        [SerializeField] UserGameHolder userGameHolder;

        [SerializeField] CantStopPlayerUI[] playerUis;
        [SerializeField] CantStopAssetManager playerColors;
        [SerializeField] Button rollButton;
        [SerializeField] Button placeButton;
        [SerializeField] Button playButton;
        [SerializeField] DiceController diceController;
        [SerializeField] GameBoardUI boardController;
        [SerializeField] WhiteConePanel whiteConePanel;
        [SerializeField] TextMeshProUGUI statText;

        Dictionary<ICantStopPlayer, CantStopPlayerUI> playerUiDict = new();

        SortedDictionary<int, (int, MarkMode)> possibleMoves = new();
        bool mustSelectMoves;

        CantStopBoardData boardData;
        IEnumerable<int> selectedNumbers = new HashSet<int>();

        ICantStopGameHandler gameHandler;
        ICantStopPlayer currentPlayer;
        ICantStopPlayer selfPlayer;
        bool isbusted;

        private void Awake()
        {
            gameHandler = userGameHolder.GetGameHandler<ICantStopGameHandler>();

            rollButton.interactable = false;
            placeButton.interactable = false;
            playButton.interactable = false;

            gameHandler.SignalGameLoaded();

            diceController.PairSelected += (v1, v2) =>
            {
                if (v1 != null)
                {
                    UpdatePossibleMoves(v1.Value, v2.Value);
                    bool allWrong = possibleMoves.All(m => m.Value.Item2 == MarkMode.Wrong);
                    placeButton.interactable = !allWrong && !isbusted && !mustSelectMoves;
                    playButton.interactable =  isbusted || (!allWrong && !mustSelectMoves);
                    boardController.PreviewColumn(v1.Value, possibleMoves[v1.Value].Item1, possibleMoves[v1.Value].Item2);
                    boardController.PreviewColumn(v2.Value, possibleMoves[v2.Value].Item1, possibleMoves[v2.Value].Item2);
                }
                else
                {
                    placeButton.interactable = false;
                    playButton.interactable = isbusted;
                    boardController.ClearMarks();
                }
            };

            Subscribe();
        }

        private void OnGameReceived(CantStopBoardData data, ICantStopPlayer startPlayer)
        {
            boardData = data;
            boardController.Initialize(data);
            int index = 0;

            foreach (var player in gameHandler.Players)
            {
                playerUis[index].SetPlayer(player);
                playerUiDict[player] = playerUis[index];

                if(player.IsMe)
                    selfPlayer = player;

                index++;
            }

            currentPlayer = startPlayer;
            playerUiDict[startPlayer].ToggleTurn(true);
            OnTurnStarted(startPlayer);
        }

        void UpdatePossibleMoves(int v1, int v2)
        {
            possibleMoves.Clear();

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

            mustSelectMoves = newWhiteCones + gameHandler.WhiteConePositions.Count() > CantStopRoomManager.whineConeLimit;

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
            if (gameHandler.FinishedColumns.Contains(columnNumber))
                return null;

            int whiteConePos = -1;
            int newCone = 0;

            if (gameHandler.WhiteConePositions.TryGetValue(columnNumber, out whiteConePos) == false) // new white cone required
            {
                if (gameHandler.WhiteConePositions.Count() >= CantStopRoomManager.whineConeLimit) // Check if we run out of white cones
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
            placeButton.onClick.AddListener(OnPlaceClicked);
            playButton.onClick.AddListener(OnPlayClicked);
            boardController.SelectedChanged += OnSelectedChanged;
        }

        private void OnPlayClicked()
        {
            var g = diceController.SelectedIndices.ToArray();

            placeButton.interactable = false;
            playButton.interactable = false;
            if(isbusted)
                 gameHandler.CancelRound();
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
                placeButton.interactable = true;
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
        }

        private async void OnRoundCanceled(ICantStopPlayer player1, ICantStopPlayer nextPlayer)
        {
            statText.text = (player1.IsMe ? "You" : player1.Name) + " Canceled the Round!";
            whiteConePanel.UpdateUI(CantStopRoomManager.whineConeLimit);
            boardController.RemoveWhiteCones();
            OnTurnStarted(nextPlayer);
            await System.Threading.Tasks.Task.Delay(2000);
            statText.text = "";
        }

        private async void OnWhiteConePlaced(ICantStopPlayer player, IDictionary<int, int> dictionary, int dice1, int dice2)
        {
            foreach (var p in dictionary)
                boardController.PlaceCone(p.Key, PlayerColor.None, p.Value);

            whiteConePanel.UpdateUI(CantStopRoomManager.whineConeLimit - gameHandler.WhiteConePositions.Count());

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

            whiteConePanel.UpdateUI(CantStopRoomManager.whineConeLimit - gameHandler.WhiteConePositions.Count());

            if (player != selfPlayer)
            {
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
        }

        private void OnTurnStarted(ICantStopPlayer player)
        {
            if(currentPlayer != null)
            {
                playerUiDict[currentPlayer].ToggleTurn(false);
            }

            currentPlayer = player;
            playerUiDict[player].ToggleTurn(true);
            rollButton.interactable = player.IsMe;
            placeButton.interactable = false;
            playButton.interactable = false;
            whiteConePanel.UpdateUI(CantStopRoomManager.whineConeLimit);
            diceController.Reset();
            boardController.RemoveWhiteCones();
            boardController.ClearMarks();
        }

        private void OnRollChanged(int[] dices, bool isBusted)
        {
            diceController.Block(currentPlayer.IsMe == false);
            diceController.SetDiceValues(dices);
            this.isbusted = isBusted;
            playButton.interactable = currentPlayer.IsMe && isbusted;
            if (isbusted)
                statText.text = (currentPlayer.IsMe ? "You are Busted!" : $"{currentPlayer.Name} is Busted!");
        }

        public void OnRollClicked()
        {
            rollButton.interactable = false;
            gameHandler.RollDice();
        }

        public void OnPlaceClicked()
        {
            placeButton.interactable = false;
            playButton.interactable = false;
            gameHandler.PlaceWhiteCones(diceController.SelectedIndices.ElementAtOrDefault(0), diceController.SelectedIndices.ElementAtOrDefault(1), selectedNumbers.Count() == 0 ? null : selectedNumbers.ElementAt(0));

            diceController.Block(true);
            diceController.Reset();
            boardController.ClearMarks();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
