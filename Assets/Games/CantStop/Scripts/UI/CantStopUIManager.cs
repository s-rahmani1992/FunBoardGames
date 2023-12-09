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

        IEnumerable<int> selectedNumbers;

        private void Awake()
        {
            rollButton.interactable = false;
            placeButton.interactable = false;
            playButton.interactable = false;
            roomManager = FindObjectOfType<CantStopRoomManager>();
            roomManager.LocalPlayer.CmdGameReady();
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
                    possibleMoves[v1] = (1, 1);

                return;
            }

            (int pos, int cone)? c1 = null;

            if (g.TryGetValue(v1, out int cx1))
                c1 = (cx1 + 1, 0);
            else if (g.Count < CantStopRoomManager.whineConeLimit)
                c1 = (0, 1);

            (int pos, int cone)? c2 = null;

            if (g.TryGetValue(v2, out int cx2))
                c2 = (cx2 + 1, 0);
            else if(g.Count < CantStopRoomManager.whineConeLimit)
                c2 = (0, 1);

            if(c1 != null) possibleMoves[v1] = c1.Value;
            if (c2 != null) possibleMoves[v2] = c2.Value;
        }

        private void Start()
        {
            rollButton.onClick.AddListener(OnRollClicked);
            placeButton.onClick.AddListener(OnPlaceClicked);
            boardController.SelectedChanged += OnSelectedChanged;
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
            diceController.Reset();
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
            }
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
                    (roomManager.LocalPlayer as CantStopPlayer).CmdPlace(g[0], g[1], selectedNumbers.ElementAt(0), null);
                    break;
                case 2:
                    (roomManager.LocalPlayer as CantStopPlayer).CmdPlace(g[0], g[1], selectedNumbers.ElementAt(0), selectedNumbers.ElementAt(1));
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
