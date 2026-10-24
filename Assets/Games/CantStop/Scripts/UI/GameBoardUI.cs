using FunBoardGames.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class GameBoardUI : MonoBehaviour
    {
        [SerializeField] GameBoardColumn[] boardColumns;
        [SerializeField] Transform holder;
        [SerializeField] ToggleGroup toggleGroup;

        SortedDictionary<int, GameBoardColumn> columnList;
        readonly Dictionary<int, GameBoardColumn> dormantList = new();
        HashSet<GameBoardColumn> selectedColumns = new();

        public event Action<IEnumerable<GameBoardColumn>> SelectedChanged;

        public void Initialize(CantStopBoardData board)
        {
            columnList = new();
            dormantList.Clear();

            var columns = board.GetColumns();

            for(int i = 2; i <= 12; i++)
            {
                if(columns.TryGetValue(i, out int count))
                {
                    boardColumns[i - 2].Initalize(i, count, toggleGroup);
                    boardColumns[i - 2].SelectChanged += OnSelectChanged;
                    columnList.Add(i, boardColumns[i - 2]);
                }
                else
                {
                    boardColumns[i - 2].InitializeDormant(i);
                    dormantList.Add(i, boardColumns[i - 2]);
                }
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

        bool TryGetColumn(int number, out GameBoardColumn column) => columnList.TryGetValue(number, out column) || dormantList.TryGetValue(number, out column);

        public void MarkColumn(int c1, MarkMode canSelect)
        {
            if (!TryGetColumn(c1, out var column))
                return;

            column.Mark(column.IsDormant ? MarkMode.Wrong : canSelect);
        }

        public void PreviewColumn(int c1, int pos, MarkMode selectMode)
        {
            if (!TryGetColumn(c1, out var column))
                return;

            if (column.IsDormant && selectMode != MarkMode.Clear)
                selectMode = MarkMode.Wrong;

            column.Mark(selectMode);

            if (selectMode == MarkMode.Correct)
            {
                column.PreviewCone(pos);
            }
            else if(selectMode == MarkMode.Select)
            {
                column.SetPreviewPosition(pos);
            }
        }

        public void ClearMarks()
        {
            foreach(var column in columnList.Values.Concat(dormantList.Values))
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
            if (columnList.TryGetValue(column, out var c))
                c.PlaceCone(color, cellNumber);
        }

        public void PreviewColumn(int number, int? pos)
        {
            if (columnList.TryGetValue(number, out var c))
                c.PreviewCone(pos);
        }
    }
}
