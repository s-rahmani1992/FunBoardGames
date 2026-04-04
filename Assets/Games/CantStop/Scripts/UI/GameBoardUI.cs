using FunBoardGames.Network;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class GameBoardUI : MonoBehaviour
    {
        [SerializeField] GameBoardColumn column;
        [SerializeField] Transform holder;
        [SerializeField] ToggleGroup toggleGroup;

        SortedDictionary<int, GameBoardColumn> columnList;

        HashSet<GameBoardColumn> selectedColumns = new();

        public event Action<IEnumerable<GameBoardColumn>> SelectedChanged;

        public void Initialize(CantStopBoardData board)
        {
            columnList = new();

            foreach(var pair in board.GetColumns())
            {
                var c = Instantiate(column, holder);
                c.Initalize(pair.Key, pair.Value, toggleGroup);
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

        public void MarkColumn(int c1, MarkMode canSelect)
        {
            columnList[c1].Mark(canSelect);
        }

        public void PreviewColumn(int c1, int pos, MarkMode selectMode)
        {
            columnList[c1].Mark(selectMode);

            if (selectMode == MarkMode.Correct)
            {
                columnList[c1].PreviewCone(pos);
            }
            else if(selectMode == MarkMode.Select)
            {
                columnList[c1].SetPreviewPosition(pos);
            }
        }

        public void ClearMarks()
        {
            foreach(var column in columnList.Values)
            {
                column.Mark(MarkMode.Clear);
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
