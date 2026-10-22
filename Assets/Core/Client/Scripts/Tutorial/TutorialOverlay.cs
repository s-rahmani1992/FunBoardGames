using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Tutorial
{
    public class TutorialOverlay : MonoBehaviour
    {
        enum MessageZone { None, Top, Center, Bottom }

        const float FadeDuration = 0.25f;

        [SerializeField] Canvas canvas;
        [SerializeField] RectTransform content;
        [SerializeField] RectTransform stepRoot;

        [Header("Dimmer")]
        [SerializeField] RectTransform dimRoot;
        [SerializeField] CanvasGroup dimGroup;
        [SerializeField] Image blocker;
        [SerializeField] Image dimCellPrefab;

        [Header("Hand")]
        [SerializeField] RectTransform hand;
        [SerializeField] Image handImage;

        [Header("Message")]
        [SerializeField] RectTransform panel;
        [SerializeField] Image panelImage;
        [SerializeField] CanvasGroup panelGroup;
        [SerializeField] TMP_Text messageText;
        [SerializeField] TMP_Text tapHintText;

        [Header("Skip")]
        [SerializeField] Button skipButton;
        [SerializeField] Image skipImage;
        [SerializeField] TMP_Text skipLabel;

        TutorialStyle style;
        Camera uiCamera;
        RectTransform handArt;
        Tween handTween;
        readonly List<Image> dimCells = new();

        readonly List<Rect> holes = new();
        readonly List<Rect> lastHoles = new();
        readonly Vector3[] corners = new Vector3[4];
        MessageZone zone = MessageZone.None;
        bool layoutDirty;

        public event Action SkipClicked;

        public static Camera CameraOf(RectTransform element)
        {
            var canvas = element.GetComponentInParent<Canvas>();
            if (canvas == null)
                return null;

            canvas = canvas.rootCanvas;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        public static bool ContainsScreenPoint(RectTransform element, Vector2 screenPoint)
        {
            return element != null && element.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(element, screenPoint, CameraOf(element));
        }

        public void Initialize(TutorialStyle style, Camera uiCamera)
        {
            this.style = style;
            this.uiCamera = uiCamera;

            canvas.renderMode = uiCamera != null ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = uiCamera;
            // Above the game's own canvases, which can override their sorting, but still below dialogs, which use the last sorting layer
            var layers = SortingLayer.layers;
            int dialogLayer = Array.FindIndex(layers, layer => layer.name == "Dialog");
            canvas.sortingLayerID = layers[dialogLayer > 0 ? dialogLayer - 1 : layers.Length - 1].id;

            handImage.sprite = style.handSprite;
            handArt = handImage.rectTransform;
            handArt.pivot = style.handTipPivot;
            handArt.sizeDelta = new Vector2(style.handSize, style.handSize);

            panelImage.sprite = style.panelSprite;
            panelImage.color = style.panelColor;
            ApplyText(messageText, style.messageFontSize, style.textColor);
            ApplyText(tapHintText, style.messageFontSize * 0.62f, style.hintTextColor);

            skipImage.sprite = style.skipButtonSprite;
            if (style.font != null)
                skipLabel.font = style.font;

            skipButton.onClick.AddListener(() => SkipClicked?.Invoke());

            hand.gameObject.SetActive(false);
            stepRoot.gameObject.SetActive(false);
        }

        void ApplyText(TMP_Text text, float size, Color color)
        {
            if (style.font != null)
                text.font = style.font;

            text.fontSize = size;
            text.color = color;
        }

        public void Show(TutorialStep step)
        {
            stepRoot.gameObject.SetActive(true);

            messageText.text = step.Message;
            tapHintText.gameObject.SetActive(step.Advance == TutorialAdvance.AnyClick);

            dimRoot.gameObject.SetActive(step.Dim);
            blocker.raycastTarget = step.Dim && step.Advance == TutorialAdvance.AnyClick;

            lastHoles.Clear();
            layoutDirty = true;
            zone = MessageZone.None;
            holes.Clear();
            PlaceMessage(step);

            dimGroup.DOKill();
            dimGroup.alpha = 0f;
            dimGroup.DOFade(1f, FadeDuration).SetLink(gameObject);

            panelGroup.DOKill();
            panelGroup.alpha = 0f;
            panelGroup.DOFade(1f, FadeDuration).SetLink(gameObject);
            panel.DOKill();
            panel.localScale = Vector3.one * 0.92f;
            panel.DOScale(1f, FadeDuration * 1.5f).SetEase(Ease.OutBack).SetLink(gameObject);

            SetHandVisible(false);
            Track(step);
        }

        public void HideStep()
        {
            SetHandVisible(false);
            dimGroup.DOKill();
            panelGroup.DOKill();
            panel.DOKill();
            stepRoot.gameObject.SetActive(false);
            lastHoles.Clear();
        }

        public void SetSkipVisible(bool visible)
        {
            skipButton.gameObject.SetActive(visible);
        }

        public void Track(TutorialStep step)
        {
            if (stepRoot.gameObject.activeSelf == false)
                return;

            CollectHoles(step);

            if (step.Dim && (layoutDirty || HolesChanged()))
            {
                layoutDirty = false;
                LayoutDimmer();
                lastHoles.Clear();
                lastHoles.AddRange(holes);
                PlaceMessage(step);
            }

            UpdateHand(step);
        }

        void CollectHoles(TutorialStep step)
        {
            holes.Clear();

            if (step.Targets == null)
                return;

            foreach (var target in step.Targets())
            {
                if (target == null || target.gameObject.activeInHierarchy == false)
                    continue;

                var rect = ToLocalRect(target);
                rect.min -= Vector2.one * style.holePadding;
                rect.max += Vector2.one * style.holePadding;
                holes.Add(rect);
            }
        }

        bool HolesChanged()
        {
            if (holes.Count != lastHoles.Count)
                return true;

            for (int i = 0; i < holes.Count; i++)
            {
                if (Vector2.SqrMagnitude(holes[i].min - lastHoles[i].min) > 0.25f
                    || Vector2.SqrMagnitude(holes[i].max - lastHoles[i].max) > 0.25f)
                    return true;
            }

            return false;
        }

        void LayoutDimmer()
        {
            Rect screen = content.rect;
            var xs = new List<float> { screen.xMin, screen.xMax };
            var ys = new List<float> { screen.yMin, screen.yMax };

            foreach (var hole in holes)
            {
                xs.Add(Mathf.Clamp(hole.xMin, screen.xMin, screen.xMax));
                xs.Add(Mathf.Clamp(hole.xMax, screen.xMin, screen.xMax));
                ys.Add(Mathf.Clamp(hole.yMin, screen.yMin, screen.yMax));
                ys.Add(Mathf.Clamp(hole.yMax, screen.yMin, screen.yMax));
            }

            xs.Sort();
            ys.Sort();

            int used = 0;

            for (int i = 0; i < xs.Count - 1; i++)
            {
                for (int j = 0; j < ys.Count - 1; j++)
                {
                    float width = xs[i + 1] - xs[i];
                    float height = ys[j + 1] - ys[j];

                    if (width < 0.01f || height < 0.01f)
                        continue;

                    var center = new Vector2(xs[i] + width / 2f, ys[j] + height / 2f);

                    if (IsInsideAnyHole(center))
                        continue;

                    var cell = GetDimCell(used++);
                    cell.rectTransform.anchoredPosition = center;
                    cell.rectTransform.sizeDelta = new Vector2(width, height);
                }
            }

            for (int i = used; i < dimCells.Count; i++)
                dimCells[i].gameObject.SetActive(false);
        }

        bool IsInsideAnyHole(Vector2 point)
        {
            foreach (var hole in holes)
            {
                if (hole.Contains(point))
                    return true;
            }

            return false;
        }

        Image GetDimCell(int index)
        {
            while (dimCells.Count <= index)
            {
                dimCells.Add(Instantiate(dimCellPrefab, dimRoot));
            }

            var image = dimCells[index];
            image.color = style.dimColor;
            image.gameObject.SetActive(true);
            return image;
        }

        void PlaceMessage(TutorialStep step)
        {
            var wanted = step.MessagePosition switch
            {
                TutorialMessagePosition.Top => MessageZone.Top,
                TutorialMessagePosition.Center => MessageZone.Center,
                TutorialMessagePosition.Bottom => MessageZone.Bottom,
                _ => AutoZone(step),
            };

            if (wanted == zone)
                return;

            zone = wanted;

            float y = wanted switch
            {
                MessageZone.Top => 1f,
                MessageZone.Bottom => 0f,
                _ => 0.5f,
            };

            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, y);
            panel.sizeDelta = new Vector2(style.panelWidth, 0f);
            // The top is kept free for the skip button
            panel.anchoredPosition = new Vector2(0f, wanted == MessageZone.Top ? -190f : wanted == MessageZone.Bottom ? style.panelMargin : 0f);
        }

        MessageZone AutoZone(TutorialStep step)
        {
            if (holes.Count == 0)
                return step.Dim ? MessageZone.Center : MessageZone.Top;

            float top = float.MinValue, bottom = float.MaxValue;
            foreach (var hole in holes)
            {
                top = Mathf.Max(top, hole.yMax);
                bottom = Mathf.Min(bottom, hole.yMin);
            }

            return (top + bottom) / 2f > 0f ? MessageZone.Bottom : MessageZone.Top;
        }

        void UpdateHand(TutorialStep step)
        {
            if (step.ShowHand == false)
            {
                SetHandVisible(false);
                return;
            }

            RectTransform target = null;

            if (step.HandTarget != null)
                target = step.HandTarget();
            else if (step.Targets != null)
            {
                foreach (var t in step.Targets())
                {
                    if (t != null && t.gameObject.activeInHierarchy)
                    {
                        target = t;
                        break;
                    }
                }
            }

            if (target == null || target.gameObject.activeInHierarchy == false)
            {
                SetHandVisible(false);
                return;
            }

            Rect rect = ToLocalRect(target);
            Rect screen = content.rect;

            // The hand comes from below and points up, unless the element is in the lower part of the screen
            bool pointDown = rect.center.y < screen.center.y - screen.height * 0.15f;
            float inset = Mathf.Min(rect.height * 0.3f, 50f);

            hand.anchoredPosition = pointDown
                ? new Vector2(rect.center.x, rect.yMax - inset)
                : new Vector2(rect.center.x, rect.yMin + inset);
            hand.localRotation = Quaternion.Euler(0f, 0f, pointDown ? 180f : 0f);

            SetHandVisible(true);
        }

        void SetHandVisible(bool visible)
        {
            if (hand.gameObject.activeSelf == visible)
                return;

            hand.gameObject.SetActive(visible);
            handTween?.Kill();

            if (visible == false)
                return;

            // The hand taps towards the pointed spot over and over
            handArt.anchoredPosition = new Vector2(0f, -40f);
            handTween = handArt.DOAnchorPosY(0f, 0.45f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }

        Rect ToLocalRect(RectTransform element)
        {
            element.GetWorldCorners(corners);
            Camera elementCamera = CameraOf(element);

            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);

            foreach (var corner in corners)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(elementCamera, corner);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(content, screenPoint, uiCamera, out var local);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
