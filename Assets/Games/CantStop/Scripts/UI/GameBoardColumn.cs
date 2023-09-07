using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class GameBoardColumn : MonoBehaviour
    {
        [SerializeField] GameObject square;
        [SerializeField] TextMeshProUGUI numberText;
        [SerializeField] Transform holder;
        [SerializeField] Transform lastCell;

        public void Initalize(int number, int count)
        {
            for (int i = 0; i < count; i++)
                Instantiate(square, holder);

            numberText.text = number.ToString();
            lastCell.SetParent(holder);
        }
    }
}
