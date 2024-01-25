using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
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
        [SerializeField] GameObject previewCone;

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

        public void PlaceCone(PlayerColor playerColor, int? number)
        {
            if (placedCones.TryGetValue(playerColor, out GameObject cone))
            {
                if (number == null)
                    cone.SetActive(false);
                else
                {
                    cone.SetActive(true);
                    cells[number.Value].AddCone(cone);
                }
            }
            else
            {
                if (number == null)
                    return;

                var newCcne = Instantiate(conePrefab, transform);
                newCcne.GetComponent<Image>().color = assetManager.GetPlayerColor(playerColor);
                placedCones.Add(playerColor, newCcne.gameObject);
                cells[number.Value].AddCone(newCcne.gameObject);
            }
        }

        public void PreviewCone(int? pos)
        {
            previewCone.SetActive(pos != null);
            previewCone.transform.position = cells[pos.GetValueOrDefault()].transform.position;
        }
    }
}
