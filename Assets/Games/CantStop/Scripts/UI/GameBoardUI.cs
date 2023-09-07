using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class GameBoardUI : MonoBehaviour
    {
        [SerializeField] GameBoardColumn column;
        [SerializeField] Transform holder;

        SortedDictionary<int, GameBoardColumn> columnList;

        GameBoardColumn marked1;
        GameBoardColumn marked2;

        public void Initialize(GameBoard board)
        {
            columnList = new();

            foreach(var pair in board.GetEnumerator())
            {
                var c = Instantiate(column, holder);
                c.Initalize(pair.Key, pair.Value);
                columnList.Add(pair.Key, c);
            }
        }

        public void MarkColumns(int c1, int c2)
        {
            ClearMarks();

            if (c1 == c2)
            {
                marked1 = marked2 = columnList[c2];
                columnList[c2].Mark(true);
                return;
            }

            marked1 = columnList[c1];
            marked2 = columnList[c2];
            marked1.Mark(true);
            marked2.Mark(true);
        }

        public void ClearMarks()
        {
            if (marked1 == null)
                return;

            marked1.Mark(null);
            marked2.Mark(null);
            marked1 = marked2 = null;
        }
    }
}
