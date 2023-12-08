using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] Toggle toggle;
        [SerializeField] Image conePrefab;
        [SerializeField] CantStopAssetManager assetManager;

        List<BoardCell> cells;
        SortedDictionary<PlayerColor, GameObject> placedCones = new();

        public int Number { get; private set; }

        public event Action<GameBoardColumn, bool> SelectChanged;

        private void Start()
        {
            toggle.onValueChanged.AddListener((isOn) => SelectChanged?.Invoke(this, isOn));
        }

        public void Initalize(int number, int count)
        {
            Mark(null);
            Number = number;
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

            correctObject.SetActive(mark.Value);
            wrongObject.SetActive(!mark.Value);
        }

        public void ResetToggle()
        {
            toggle.SetIsOnWithoutNotify(false);
        }

        public void PlaceCone(PlayerColor playerColor, int number)
        {
            if (placedCones.TryGetValue(playerColor, out GameObject cone))
                cells[number].AddCone(cone);
            else
            { 
                var newCcne = Instantiate(conePrefab, transform);
                newCcne.GetComponent<Image>().color = assetManager.GetPlayerColor(playerColor);
                placedCones.Add(playerColor, newCcne.gameObject);
                cells[number].AddCone(newCcne.gameObject);
            }
        }
    }
}
