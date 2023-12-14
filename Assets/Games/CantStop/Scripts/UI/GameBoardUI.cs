using System;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class GameBoardUI : MonoBehaviour
    {
        [SerializeField] GameBoardColumn column;
        [SerializeField] Transform holder;

        SortedDictionary<int, GameBoardColumn> columnList;

        HashSet<GameBoardColumn> selectedColumns = new();

        public event Action<IEnumerable<GameBoardColumn>> SelectedChanged;

        public void Initialize(GameBoard board)
        {
            columnList = new();

            foreach(var pair in board.GetEnumerator())
            {
                var c = Instantiate(column, holder);
                c.Initalize(pair.Key, pair.Value);
                c.SelectChanged += OnSelectChanged;
                columnList.Add(pair.Key, c);
            }
        }

        private void OnSelectChanged(GameBoardColumn column, bool selected)
        {
            if (selected)
                selectedColumns.Add(column);
            else
                selectedColumns.Remove(column);

            SelectedChanged?.Invoke(selectedColumns);
        }

        public void MarkColumn(int c1, bool canSelect)
        {
            columnList[c1].Mark(canSelect);
        }

        public void ClearMarks()
        {
            foreach(var column in columnList.Values)
            {
                column.Mark(null);
                column.ResetToggle();
                column.PreviewCone(null);
            }
            selectedColumns.Clear();
        }

        public void RemoveWhiteCones()
        {
            foreach (var column in columnList.Values)
                column.PlaceCone(PlayerColor.None, null);
        }

        public void PlaceCone(int column, PlayerColor color, int? cellNumber)
        {
            columnList[column].PlaceCone(color, cellNumber);
        }

        public void PreviewColumn(int number, int? pos) => columnList[number].PreviewCone(pos);
    }
}
