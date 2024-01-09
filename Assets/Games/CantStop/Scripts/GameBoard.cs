using FishNet.Serializing;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    [CreateAssetMenu]
    public class GameBoard : ScriptableObject
    {
        [SerializeField] SerializableDictionary<int, int> columns;

        public int this[int index] => columns[index];

        public int ColumnCount => columns.Count;

        public IEnumerable<KeyValuePair<int, int>> GetEnumerator() => columns.GetEnumerator();

        public void Initialize(IDictionary<int, int> dictionary) => columns = new(dictionary);
    }

    public static class GameBoardExtention
    {
        public static GameBoard ReadGameBoard(this Reader reader)
        {
            int length = reader.ReadInt32();

            if (length == -1)
                return null;

            SortedDictionary<int, int> read = new();

            for(int i = 0; i < length; i++)
            {
                int k = reader.ReadInt32();
                int v = reader.ReadInt32();
                read.Add(k, v);
            }

            GameBoard board = ScriptableObject.CreateInstance<GameBoard>(); ;
            board.Initialize(read);
            return board;
        }

        public static void WriteGameBoard(this Writer writer, GameBoard board)
        {
            if (board == null)
            {
                writer.WriteInt32(-1);
                return;
            }

            writer.WriteInt32(board.ColumnCount);

            foreach(var pair in board.GetEnumerator())
            {
                writer.WriteInt32(pair.Key);
                writer.WriteInt32(pair.Value);
            }
        }
    }
}
