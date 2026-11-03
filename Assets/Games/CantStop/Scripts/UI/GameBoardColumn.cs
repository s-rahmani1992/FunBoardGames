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

        const float NewConeStartScale = 0.4f;

        List<BoardCell> cells;
        WhiteConePanel panel;
        GhostPulse ghost;

        // The marker is taken from the panel while it is on the board; confirmedPos is where the game says it is, markerPos where it is shown
        GameObject whiteCone;
        int? markerPos;
        int? confirmedPos;
        int? choicePos;

        GhostPulse Ghost => ghost != null ? ghost : (ghost = previewCone.GetComponent<GhostPulse>());
        SortedDictionary<PlayerColor, GameObject> placedCones = new();

        public int Number { get; private set; }

        public bool IsDormant { get; private set; }

        public event Action<GameBoardColumn, bool> SelectChanged;

        private void Start()
        {
            toggle.onValueChanged.AddListener((isOn) =>
            {
                Ghost.SetStyle(isOn ? GhostStyle.Chosen : GhostStyle.Choice);
                choiceFrame.SetActive(!isOn);
                previewCone.SetActive(!isOn);

                if (isOn && choicePos != null)
                    MoveMarker(choicePos.Value);
                else
                    RevertMarker();

                SelectChanged?.Invoke(this, isOn);
            });
        }

        public void Initalize(int number, int count, ToggleGroup toggleGroup, WhiteConePanel conePanel)
        {
            panel = conePanel;
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

        public void InitializeDormant(int number)
        {
            IsDormant = true;
            Mark(MarkMode.Clear);
            Number = number;
            cells = new();

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

        /// <summary>A new cone of a player starts at the spawn position, when there is one.</summary>
        public void PlaceCone(PlayerColor playerColor, int? number, Vector3? spawnPosition = null)
        {
            if (playerColor == PlayerColor.None)
            {
                confirmedPos = number;

                if (number == null)
                    ReturnMarker();
                else
                    MoveMarker(number.Value);

                return;
            }

            if (placedCones.TryGetValue(playerColor, out GameObject cone))
            {
                if (number == null)
                {
                    ConeMover.Of(cone).Release();
                    cone.SetActive(false);
                }
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

                var newCone = Instantiate(conePrefab, transform);
                newCone.GetComponent<Image>().color = assetManager.GetPlayerColor(playerColor);

                var rect = (RectTransform)newCone.transform;
                rect.sizeDelta = panel.ConeSize;

                if (spawnPosition != null)
                {
                    rect.position = spawnPosition.Value;
                    rect.localScale = Vector3.one * NewConeStartScale;
                }

                placedCones.Add(playerColor, newCone.gameObject);
                // A slow start, so the cone is seen leaving the player
                cells[number.Value].AddCone(newCone.gameObject, true, DG.Tweening.Ease.InOutQuad);
            }

            if(!finishObject.activeSelf)
                finishObject.SetActive(number == cells.Count - 1);
        }

        public void MoveMarker(int pos)
        {
            if (cells.Count == 0)
                return;

            if (whiteCone == null)
            {
                whiteCone = panel.PullMarker();

                if (whiteCone == null)
                    return;
            }

            whiteCone.SetActive(true);
            markerPos = pos;
            cells[pos].AddCone(whiteCone);
        }

        public void RevertMarker()
        {
            if (confirmedPos != null)
                MoveMarker(confirmedPos.Value);
            else
                ReturnMarker();
        }

        public void CommitMarker()
        {
            if (markerPos != null)
                confirmedPos = markerPos;
        }

        public void RemoveMarker()
        {
            confirmedPos = null;
            ReturnMarker();
        }

        void ReturnMarker()
        {
            if (whiteCone == null)
                return;

            panel.PushMarker(whiteCone);
            whiteCone = null;
            markerPos = null;
        }

        public void HidePreview()
        {
            choicePos = null;
            Ghost.SetStyle(GhostStyle.Preview);
            previewCone.SetActive(false);
        }

        public void SetPreviewPosition(int? pos)
        {
            if (cells.Count == 0)
                return;

            choicePos = pos;
            var cellPosition = cells[pos.GetValueOrDefault()].transform.position;
            previewCone.transform.position = cellPosition;
            selectObject.transform.position = cellPosition;
            choiceFrame.SetActive(true);
            previewCone.SetActive(true);
            Ghost.SetStyle(GhostStyle.Choice);
        }
    }
}
