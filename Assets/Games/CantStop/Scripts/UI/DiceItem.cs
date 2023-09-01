using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.CantStop
{
    public class DiceItem : MonoBehaviour
    {
        [SerializeField] Sprite[] diceSprites;
        [SerializeField] Image icon;

        public void SetValue(int value) => icon.sprite = diceSprites[value];
    }
}
