using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.CantStop
{
    public class DiceItem : MonoBehaviour
    {
        [field: SerializeField] public int Index { get; private set; }
        [SerializeField] Sprite[] diceSprites;
        [SerializeField] Image icon;
        [SerializeField] GameObject highlightObject;

        public int Value { get; private set; }

        public event Action<DiceItem> Clicked;

        public void SetValue(int value)
        {
            Value = value;
            icon.sprite = diceSprites[Value];
        }

        public void OnClick() => Clicked?.Invoke(this);

        public void Highlight(bool highlight) => highlightObject.SetActive(highlight);
    }
}
