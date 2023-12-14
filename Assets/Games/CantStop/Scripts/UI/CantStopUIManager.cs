using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.CantStop
{
    public class CantStopUIManager : MonoBehaviour
    {
        CantStopRoomManager roomManager;

        [SerializeField] CantStopPlayerUI[] playerUis;
        [SerializeField] CantStopAssetManager playerColors;
        [SerializeField] Button rollButton;
        [SerializeField] Button placeButton;
        [SerializeField] Button playButton;
        [SerializeField] DiceController diceController;
        [SerializeField] GameBoardUI boardController;
        [SerializeField] WhiteConePanel whiteConePanel;

        SortedDictionary<int, (int pos, int cone)> possibleMoves = new();
        CantStopPlayer localPlayer;
        IEnumerable<int> selectedNumbers;

        private void Awake()
        {
            rollButton.interactable = false;
            placeButton.interactable = false;
            playButton.interactable = false;
            roomManager = FindObjectOfType<CantStopRoomManager>();
            roomManager.LocalPlayer.CmdGameReady();
            localPlayer = roomManager.LocalPlayer as CantStopPlayer;
            diceController.Block(!roomManager.IsYourTurn);
            diceController.PairSelected += (v1, v2) =>
            {
                if (!roomManager.IsYourTurn)
                    return;

                playButton.interactable = v1 != null;

                if (v1 != null)
                {
                    CheckMove(v1.Value, v2.Value);
                    boardController.MarkColumn(v1.Value, possibleMoves.ContainsKey(v1.Value));
                    boardController.MarkColumn(v2.Value, possibleMoves.ContainsKey(v2.Value));
                }
                else
                    boardController.ClearMarks();
            };

            Subscribe();
        }

        void CheckMove(int v1, int v2)
        {
            var g = roomManager.WhiteConePositions;
            possibleMoves.Clear();

            if(v1 == v2)
            {
                if (g.TryGetValue(v1, out int c))
                    possibleMoves[v1] = (c + 2, 0);
                else if(g.Count < CantStopRoomManager.whineConeLimit)
                {
                    if(localPlayer.ConePositions.TryGetValue(v1, out int pos))
                        possibleMoves[v1] = (pos + 2, 1);
                    else
                        possibleMoves[v1] = (1, 1);
                }
                    
                return;
            }

            (int pos, int cone)? c1 = GetPosibleMove(v1);
            (int pos, int cone)? c2 = GetPosibleMove(v2);

            if(c1 != null) possibleMoves[v1] = c1.Value;
            if (c2 != null) possibleMoves[v2] = c2.Value;
        }

        (int pos, int cone)? GetPosibleMove(int columnNumber)
        {
            var g = roomManager.WhiteConePositions;

            if (g.TryGetValue(columnNumber, out int whiteCoePos))
                return (whiteCoePos + 1, 0);
            else if (g.Count < CantStopRoomManager.whineConeLimit)
            {
                if (localPlayer.ConePositions.TryGetValue(columnNumber, out int conePos))
                    return (conePos + 1, 1);
                else
                    return (0, 1);
            }

            return null;
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

            foreach (var p in possibleMoves)
                boardController.PreviewColumn(p.Key, null);

            foreach (var p in columns)
                boardController.PreviewColumn(p.Number, possibleMoves[p.Number].pos);

            whiteConePanel.UpdateUI(CantStopRoomManager.whineConeLimit - roomManager.WhiteConePositions.Count()- columns.Sum(p => possibleMoves[p.Number].cone));

            if (columns.Count() == 0)
            {
                foreach (var p in possibleMoves)
                    boardController.MarkColumn(p.Key, true);

                placeButton.interactable = false;
                return;
            }

            if(columns.Count() == 1)
            {
                placeButton.interactable = true;

                if(possibleMoves.Count == 2)
                {
                    var c = possibleMoves[columns.ElementAt(0).Number];

                    var pair = possibleMoves.FirstOrDefault(x => x.Key != columns.ElementAt(0).Number);
                    if (c.cone == 1)
                        boardController.MarkColumn(pair.Key, c.cone + pair.Value.cone + roomManager.WhiteConePositions.Count <= CantStopRoomManager.whineConeLimit);
                }

                return;
            }

            placeButton.interactable = true;
        }

        void Subscribe()
        {
            Unsubscribe();
            roomManager.GameBegin += OnGameBegin;
            roomManager.TurnStarted += OnTurnStarted;
            roomManager.DiceSelected += OnRivalDiceSelected;
            roomManager.WhiteConeMoved += OnMovedWhiteCone;
        }

        private void OnRivalDiceSelected(int index1, int index2)
        {
            diceController.PickDices(index1, index2);
        }

        void Unsubscribe()
        {
            roomManager.GameBegin -= OnGameBegin;
            roomManager.TurnStarted -= OnTurnStarted;
            roomManager.WhiteConeMoved -= OnMovedWhiteCone;
        }

        private void OnMovedWhiteCone(int number, int position)
        {
            boardController.PlaceCone(number, PlayerColor.None, position);
        }

        private void OnTurnStarted(CantStopPlayer player)
        {
            rollButton.interactable = player.hasAuthority;
            placeButton.interactable = false;
            playButton.interactable = false;
            diceController.Reset();
            boardController.RemoveWhiteCones();
            boardController.ClearMarks();
        }

        private void OnRollChanged(DiceData data)
        {
            if (!data.IsValid)
            {
                possibleMoves.Clear();
                diceController.Reset();
                diceController.Block(true);
                placeButton.interactable = false;
                rollButton.interactable = roomManager.LocalPlayer == roomManager.CurrentTurnPlayer;
                whiteConePanel.UpdateUI(CantStopRoomManager.whineConeLimit - roomManager.WhiteConePositions.Count());
                return;
            }

            diceController.Block(!roomManager.IsYourTurn);
            diceController.SetDiceValues(data);
        }

        private void OnGameBegin()
        {
            boardController.Initialize(roomManager.Board);

            foreach(var player in roomManager.PlayerList)
            {
                playerUis[player.Index - 1].SetPlayer(player, playerColors.GetPlayerColor(player.PlayerColor));
                player.RollChanged += OnRollChanged;
                player.ConePositionChanged += OnConePositionChanged;
            }
        }

        private void OnConePositionChanged(CantStopPlayer player, int c, int p)
        {
            boardController.PlaceCone(c, PlayerColor.None, null);
            boardController.PlaceCone(c, player.PlayerColor, p);
        }

        public void OnRollClicked()
        {
            rollButton.interactable = false;
            (roomManager.LocalPlayer as CantStopPlayer).CmdRoll();
        }

        public void OnPlaceClicked()
        {
            var g = diceController.SelectedIndices.ToArray();

            switch (selectedNumbers.Count())
            {
                case 0:
                    return;
                case 1:
                    localPlayer.CmdPlace(new(g[0], g[1], selectedNumbers.ElementAt(0), null));
                    break;
                case 2:
                    localPlayer.CmdPlace(new(g[0], g[1], selectedNumbers.ElementAt(0), selectedNumbers.ElementAt(1)));
                    break;
            }

            diceController.Block(true);
            boardController.ClearMarks();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
