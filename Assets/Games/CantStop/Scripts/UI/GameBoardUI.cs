using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class GameBoardUI : MonoBehaviour
    {
        [SerializeField] GameBoardColumn column;
        [SerializeField] Transform holder;
        
        public void Initialize(GameBoard board)
        {
            foreach(var pair in board.GetEnumerator())
            {
                var c = Instantiate(column, holder);
                c.Initalize(pair.Key, pair.Value);
            }
        }
    }
}
