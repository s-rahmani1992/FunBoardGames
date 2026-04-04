using FunBoardGames.Network;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class CantStopUIManager : MonoBehaviour
    {
        CantStopRoomManager roomManager;

        [SerializeField] UserGameHolder userGameHolder;

        [SerializeField] CantStopPlayerUI[] playerUis;
        [SerializeField] CantStopAssetManager playerColors;
        [SerializeField] Button rollButton;
        [SerializeField] Button placeButton;
        [SerializeField] Button playButton;
        [SerializeField] DiceController diceController;
        [SerializeField] GameBoardUI boardController;
        [SerializeField] WhiteConePanel whiteConePanel;

        Dictionary<ICantStopPlayer, CantStopPlayerUI> playerUiDict = new();

        SortedDictionary<int, (int, MarkMode)> possibleMoves = new();
        bool mustSelectMoves;

        CantStopPlayer localPlayer;
        CantStopBoardData boardData;
        IEnumerable<int> selectedNumbers = new HashSet<int>();

        ICantStopGameHandler gameHandler;
        ICantStopPlayer currentPlayer;
        ICantStopPlayer selfPlayer;

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
                    placeButton.interactable = !mustSelectMoves;
                    boardController.PreviewColumn(v1.Value, possibleMoves[v1.Value].Item1, possibleMoves[v1.Value].Item2);
                    boardController.PreviewColumn(v2.Value, possibleMoves[v2.Value].Item1, possibleMoves[v2.Value].Item2);
                }
                else
                {
                    placeButton.interactable = false;
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

                if(move == null || move.Value.pos >= boardData[v1] - 2)
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

            switch (selectedNumbers.Count())
            {
                case 0:
                    localPlayer.CmdPlay(null);
                    return;
                case 1:
                    localPlayer.CmdPlay(new PlayerPlayData(g[0], g[1], selectedNumbers.ElementAt(0), null));
                    break;
                case 2:
                    localPlayer.CmdPlay(new PlayerPlayData(g[0], g[1], selectedNumbers.ElementAt(0), selectedNumbers.ElementAt(1)));
                    break;
            }

            diceController.Block(true);
            boardController.ClearMarks();
        }

        private void OnSelectedChanged(IEnumerable<GameBoardColumn> columns)
        {
            selectedNumbers = columns.Select(c => c.Number);

            if (columns.Count() == 1)
            {
                placeButton.interactable = true;
                return;
            }
        }

        void Subscribe()
        {
            Unsubscribe();
            gameHandler.GameDataReceived += OnGameReceived;
            gameHandler.DiceRolled += OnRollChanged;
            gameHandler.WhiteConesPlaced += OnWhiteConePlaced;
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

        void Unsubscribe()
        {
            gameHandler.GameDataReceived -= OnGameReceived;
            gameHandler.DiceRolled -= OnRollChanged;
            gameHandler.WhiteConesPlaced -= OnWhiteConePlaced;
        }

        private void OnTurnStarted(ICantStopPlayer player)
        {
            rollButton.interactable = player.IsMe;
            placeButton.interactable = false;
            playButton.interactable = false;
            diceController.Reset();
            boardController.RemoveWhiteCones();
            boardController.ClearMarks();
        }

        private void OnRollChanged(int[] dices)
        {
            diceController.Block(currentPlayer.IsMe == false);
            diceController.SetDiceValues(dices);
        }

        private void OnConePositionChanged(CantStopPlayer player, int c, int p)
        {
            boardController.PlaceCone(c, PlayerColor.None, null);
            boardController.PlaceCone(c, player.PlayerColor, p);
        }

        public void OnRollClicked()
        {
            rollButton.interactable = false;
            gameHandler.RollDice();
        }

        public void OnPlaceClicked()
        {
            placeButton.interactable = false;
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
