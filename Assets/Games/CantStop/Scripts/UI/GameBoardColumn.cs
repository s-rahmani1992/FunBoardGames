using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class GameBoardColumn : MonoBehaviour
    {
        [SerializeField] BoardCell square;
        [SerializeField] TextMeshProUGUI numberText;
        [SerializeField] Transform holder;
        [SerializeField] Transform lastCell;
        [SerializeField] GameObject correctObject;
        [SerializeField] GameObject wrongObject;

        List<BoardCell> cells;

        public void Initalize(int number, int count)
        {
            Mark(null);
            cells = new();

            for (int i = 0; i < count; i++)
                cells.Add(Instantiate(square, holder));

            numberText.text = number.ToString();
            lastCell.SetParent(holder);
        }

        public void Mark(bool? mark)
        {
            if(mark == null)
            {
                correctObject.SetActive(false);
                wrongObject.SetActive(false);
                return;
            }

            correctObject.SetActive(mark.GetValueOrDefault());
            wrongObject.SetActive(!mark.GetValueOrDefault());
        }
    }
}
