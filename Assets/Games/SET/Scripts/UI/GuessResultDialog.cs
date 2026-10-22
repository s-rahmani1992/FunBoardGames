using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET
{
    public class GuessResultDialog : BaseDialog
    {
        static readonly string[] AttributeNames = { "Color", "Shape", "Number", "Shading" };

        [SerializeField] Transform[] cardHolders;
        [SerializeField] Image markImage;
        [SerializeField] Sprite checkSprite, crossSprite;
        [SerializeField] TMP_Text titleText, detailText;
        [SerializeField] Color setColor = new(0.55f, 0.9f, 0.6f);
        [SerializeField] Color notSetColor = new(1f, 0.45f, 0.35f);
        [SerializeField] float showDuration = 3f;
        [SerializeField] float flyDuration = 0.45f;
        [SerializeField] float returnDuration = 0.25f;

        CardUI[] selectedCards;
        byte guessResult;
        RectTransform targetPanel;
        Action onCardsArrived;
        Tween closeTimer;
        bool isClosing;

        /// <summary>Raised once the cards have been handled, right before the dialog closes.</summary>
        public event Action Closed;

        protected override void Awake()
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            base.Awake();
        }

        public static GuessResultDialog Show(CardUI[] cards, byte result, RectTransform targetPanel = null, Action onCardsArrived = null)
        {
            return DialogManager.Instance.SpawnDialog<GuessResultDialog>(DialogShowOptions.OverAll, (d) =>
            {
                GuessResultDialog dialog = (GuessResultDialog)d;
                dialog.selectedCards = cards;
                dialog.guessResult = result;
                dialog.targetPanel = targetPanel;
                dialog.onCardsArrived = onCardsArrived;
            });
        }

        public override void Show()
        {
            base.Show();

            bool isSet = CardData.IsSET(guessResult);
            markImage.sprite = isSet ? checkSprite : crossSprite;
            markImage.gameObject.SetActive(false);

            titleText.text = isSet ? "It's a SET!" : "Not a SET";
            titleText.color = isSet ? setColor : notSetColor;
            detailText.text = isSet ? string.Empty : $"Mismatch: {MismatchedAttributes()}";
            titleText.gameObject.SetActive(false);
            detailText.gameObject.SetActive(false);

            for (int i = 0; i < selectedCards.Length; i++)
            {
                selectedCards[i].transform.SetParent(cardHolders[i]);
                var move = selectedCards[i].transform.DOLocalMove(Vector3.zero, 0.5f);

                if (i == selectedCards.Length - 1)
                    move.OnComplete(ShowVerdict);
            }

            closeTimer = DOVirtual.DelayedCall(showDuration, Close);
        }

        void ShowVerdict()
        {
            if (isClosing)
                return;

            markImage.gameObject.SetActive(true);
            markImage.transform.localScale = Vector3.zero;
            markImage.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            titleText.gameObject.SetActive(true);
            detailText.gameObject.SetActive(detailText.text.Length > 0);
        }

        string MismatchedAttributes()
        {
            var comparison = CardData.Byte2Result(guessResult);
            var mismatched = new List<string>();

            for (int i = 0; i < comparison.Length; i++)
            {
                if (comparison[i] == TripleComparisonResult.NONE)
                    mismatched.Add(AttributeNames[i]);
            }

            return string.Join(", ", mismatched);
        }

        public override void Close()
        {
            if (isClosing)
                return;

            isClosing = true;
            closeTimer?.Kill();

            markImage.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack);
            titleText.gameObject.SetActive(false);
            detailText.gameObject.SetActive(false);

            if (CardData.IsSET(guessResult))
                FlyCardsToPanel();
            else
                ReturnCardsToTable();
        }

        void FlyCardsToPanel()
        {
            Vector3 target = targetPanel != null ? targetPanel.TransformPoint(targetPanel.rect.center) : transform.position;
            var fly = DOTween.Sequence();

            foreach (var card in selectedCards)
            {
                fly.Join(card.transform.DOMove(target, flyDuration).SetEase(Ease.InBack));
                fly.Join(card.transform.DOScale(0.15f, flyDuration).SetEase(Ease.InQuad));
            }

            fly.OnComplete(() =>
            {
                onCardsArrived?.Invoke();

                foreach (var card in selectedCards)
                    Destroy(card.gameObject);

                Finish();
            });
        }

        void ReturnCardsToTable()
        {
            foreach (var card in selectedCards)
            {
                card.MoveBack();
                card.Mark(false);
            }

            DOVirtual.DelayedCall(returnDuration, () =>
            {
                onCardsArrived?.Invoke();
                Finish();
            });
        }

        void Finish()
        {
            Closed?.Invoke();
            base.Close();
        }

        private void OnDestroy()
        {
            closeTimer?.Kill();
        }
    }
}
