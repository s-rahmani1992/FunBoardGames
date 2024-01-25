using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FunBoardGames.CantStop
{
    public class DiceController : MonoBehaviour
    {
        [SerializeField] DiceItem[] dices;
        [SerializeField] TextMeshProUGUI number1Text;
        [SerializeField] TextMeshProUGUI number2Text;
        [SerializeField] GameObject blockObject;

        List<DiceItem> selectedDices = new();
        List<DiceItem> unselectedDices = new();

        public IEnumerable<int> SelectedIndices => selectedDices.Select(dice => dice.Index);

        public event Action<int?, int?> PairSelected;

        private void Start()
        {
            foreach(var dice in dices)
            {
                dice.Highlight(false);
                dice.Clicked += OnDiceClicked;
            }

            Reset();
        }

        public void Reset()
        {
            unselectedDices = new(dices);
            selectedDices.Clear();
            number1Text.text = number2Text.text = "";
            ClearDices();
            PairSelected?.Invoke(null, null);
        }

        private void OnDiceClicked(DiceItem dice)
        {
            if (selectedDices.Contains(dice))
            {
                dice.Highlight(false);
                selectedDices.Remove(dice);
                unselectedDices.Add(dice);
            }
            else
            {
                if (selectedDices.Count == 2)
                    return;

                dice.Highlight(true);
                unselectedDices.Remove(dice);
                selectedDices.Add(dice);
            }

            CheckSelectedDices();
        }

        void CheckSelectedDices()
        {
            if (selectedDices.Count == 2)
            {
                int p1 = selectedDices[0].Value + selectedDices[1].Value;
                int p2 = unselectedDices[0].Value + unselectedDices[1].Value;
                number1Text.text = p1.ToString();
                number2Text.text = p2.ToString();
                PairSelected?.Invoke(p1, p2);
            }
            else
            {
                number1Text.text = number2Text.text = "";
                PairSelected?.Invoke(null, null);
            }
        }

        public void SetDiceValues(DiceData diceData)
        {
            for (int i = 0; i < dices.Length; i++)
                dices[i].SetValue(diceData[i]);
        }

        public void ClearDices()
        {
            foreach (var dice in dices)
            {
                dice.SetValue(0);
                dice.Highlight(false);
            }
        }

        public void PickDices(int index1, int index2)
        {
            dices[index1].Highlight(true);
            dices[index2].Highlight(true);
        }

        public void Block(bool block) => blockObject.SetActive(block);
    }
}
