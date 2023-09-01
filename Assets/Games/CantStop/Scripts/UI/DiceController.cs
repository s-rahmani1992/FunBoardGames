using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class DiceController : MonoBehaviour
    {
        [SerializeField] DiceItem[] dices;
        
        public void SetDiceValues(DiceData diceData)
        {
            for (int i = 0; i < dices.Length; i++)
                dices[i].SetValue(diceData[i]);
        }

        public void Clear()
        {
            foreach (var dice in dices)
                dice.SetValue(0);
        }
    }
}
