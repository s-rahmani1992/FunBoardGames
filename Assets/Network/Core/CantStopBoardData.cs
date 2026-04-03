
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public class CantStopBoardData
    {
        public CantStopBoardData(IDictionary<int, int> columns) 
        { 
            this.columns = new(columns); 
        }

        SortedDictionary<int, int> columns;

        public int this[int index] => columns[index];

        public IDictionary<int, int> GetColumns() => columns;


        public int ColumnCount => columns.Count;
    }
}
