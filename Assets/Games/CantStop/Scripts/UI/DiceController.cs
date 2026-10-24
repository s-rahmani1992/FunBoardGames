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
            SetSums(null, null);
            ClearDices();
        }

        private void OnDiceClicked(DiceItem dice)
        {
            if (selectedDices.Contains(dice))
            {
                selectedDices.Remove(dice);
                unselectedDices.Add(dice);
            }
            else
            {
                if (selectedDices.Count == 2)
                    return;

                unselectedDices.Remove(dice);
                selectedDices.Add(dice);
            }

            RefreshDiceStates();
            CheckSelectedDices();
        }

        void RefreshDiceStates()
        {
            foreach (var dice in dices)
            {
                if (selectedDices.Contains(dice))
                    dice.SetState(DiceVisualState.Selected);
                else
                    dice.SetState(selectedDices.Count == 2 ? DiceVisualState.Partner : DiceVisualState.None);
            }
        }

        void CheckSelectedDices()
        {
            if (selectedDices.Count == 2)
            {
                int p1 = selectedDices[0].Value + selectedDices[1].Value;
                int p2 = unselectedDices[0].Value + unselectedDices[1].Value;
                SetSums(p1, p2);
                PairSelected?.Invoke(p1, p2);
            }
            else
            {
                SetSums(null, null);
                PairSelected?.Invoke(null, null);
            }
        }

        void SetSums(int? sum1, int? sum2)
        {
            number1Text.text = sum1?.ToString() ?? "-";
            number2Text.text = sum2?.ToString() ?? "-";
        }

        public void StartRoll()
        {
            foreach (var dice in dices)
                dice.StartRoll();
        }

        public void SetDiceValues(int[] diceData)
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

        public void ClearSelection()
        {
            foreach (var dice in dices)
            {
                dice.Highlight(false);
            }
        }

        public void PickDices(int index1, int index2)
        {
            for (int i = 0; i < dices.Length; i++)
                dices[i].SetState(i == index1 || i == index2 ? DiceVisualState.Selected : DiceVisualState.Partner);
        }

        public void Block(bool block) => blockObject.SetActive(block);
    }
}
