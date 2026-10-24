using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public enum MarkMode
    {
        Correct,
        Wrong, 
        Select,
        Clear,
    }

    public class GameBoardColumn : MonoBehaviour
    {
        [SerializeField] BoardCell square;
        [SerializeField] TextMeshProUGUI numberText;
        [SerializeField] Transform holder;
        [SerializeField] BoardCell goalSquare;
        [SerializeField] GameObject correctObject;
        [SerializeField] GameObject wrongObject;
        [SerializeField] GameObject selectObject;
        [SerializeField] Toggle toggle;
        [SerializeField] Image conePrefab;
        [SerializeField] CantStopAssetManager assetManager;
        [SerializeField] GameObject previewCone;
        [SerializeField] GameObject choiceFrame;
        [SerializeField] GameObject finishObject;
        [SerializeField] Graphic numberBadge;
        [SerializeField] Color dormantBadgeColor = new(1f, 1f, 1f, 0.35f);

        List<BoardCell> cells;
        GhostPulse ghost;

        GhostPulse Ghost => ghost != null ? ghost : (ghost = previewCone.GetComponent<GhostPulse>());
        SortedDictionary<PlayerColor, GameObject> placedCones = new();

        public int Number { get; private set; }

        public bool IsDormant { get; private set; }

        public event Action<GameBoardColumn, bool> SelectChanged;

        private void Start()
        {
            toggle.onValueChanged.AddListener((isOn) =>
            {
                // The ghost marker stays visible while the player must choose; choosing it makes it solid
                Ghost.SetStyle(isOn ? GhostStyle.Chosen : GhostStyle.Choice);
                choiceFrame.SetActive(!isOn);
                SelectChanged?.Invoke(this, isOn);
            });
        }

        public void Initalize(int number, int count, ToggleGroup toggleGroup)
        {
            Mark(MarkMode.Clear);
            toggle.group = toggleGroup;
            toggleGroup.RegisterToggle(toggle);
            Number = number;

            cells = new() { square };

            for (int i = 0; i < count - 2; i++)
                cells.Add(Instantiate(square, holder));

            goalSquare.transform.SetAsLastSibling();
            cells.Add(goalSquare);

            numberText.text = number.ToString();
        }

        /// <summary>Builds a column that is absent from the game: only its dimmed number is shown.</summary>
        public void InitializeDormant(int number)
        {
            IsDormant = true;
            Mark(MarkMode.Clear);
            Number = number;
            cells = new();

            // The holder's cells are not used, so the column is just its dimmed number
            square.gameObject.SetActive(false);
            goalSquare.gameObject.SetActive(false);
            finishObject.SetActive(false);

            numberText.text = number.ToString();
            numberText.alpha = dormantBadgeColor.a;
            numberBadge.color = dormantBadgeColor;
        }

        public void Mark(MarkMode mark)
        {
            if(mark == MarkMode.Clear)
            {
                correctObject.SetActive(false);
                wrongObject.SetActive(false);
                selectObject.SetActive(false);
                return;
            }

            correctObject.SetActive(mark == MarkMode.Correct);
            wrongObject.SetActive(mark == MarkMode.Wrong);
            selectObject.SetActive(mark == MarkMode.Select);
        }

        public void ResetToggle()
        {
            toggle.SetIsOnWithoutNotify(false);
            Ghost.SetStyle(GhostStyle.Preview);
            choiceFrame.SetActive(true);
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

            if(playerColor != PlayerColor.None && !finishObject.activeSelf)
                finishObject.SetActive(number == cells.Count - 1);
        }

        public void PreviewCone(int? pos)
        {
            Ghost.SetStyle(GhostStyle.Preview);

            // A dormant column has no cells to preview in
            if (cells.Count == 0)
            {
                previewCone.SetActive(false);
                return;
            }

            previewCone.SetActive(pos != null);
            previewCone.transform.position = cells[pos.GetValueOrDefault()].transform.position;
        }

        /// <summary>Shows the ghost marker in the cell the player can choose; the select toggle sits on that cell, so tapping the ghost chooses the column.</summary>
        public void SetPreviewPosition(int? pos)
        {
            if (cells.Count == 0)
                return;

            var cellPosition = cells[pos.GetValueOrDefault()].transform.position;
            previewCone.transform.position = cellPosition;
            selectObject.transform.position = cellPosition;
            choiceFrame.SetActive(true);
            previewCone.SetActive(true);
            Ghost.SetStyle(GhostStyle.Choice);
        }
    }
}
