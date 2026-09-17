using DG.Tweening;
using FunBoardGames.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FunBoardGames.SET
{
    public class CardDeckManager : MonoBehaviour
    {
        [SerializeField] Transform cardContainer;
        [SerializeField] Transform deckHolder;
        [SerializeField] Transform[] cardHolders;
        [SerializeField] GameObject block;
        [SerializeField] TMPro.TMP_Text remainTxt;
        [SerializeField] CardUI cardUIPrefab;
        [SerializeField] float timeoutFadeDuration = 0.5f;

        ISETGameHandler gameHandler;
        SETGameData gameData;

        public event Action CardDestributionEnded;

        public HashSet<CardUI> selectedCards = new(3);
        List<CardUI> placedCards = new();
        List<CardData> pendingCards = new();
        int remained = 1;

        bool isShowingGuessResult = false;
        List<CardData> pendingTimeoutCards = null;

        public void RegisterGameHandler(ISETGameHandler gameHandler, SETGameData gameData)
        {
            this.gameHandler = gameHandler;
            this.gameData = gameData;

            remained = 1;

            for (int i = 0; i < gameData.AttributeCount; i++)
                remained *= 3;

            remainTxt.text = remained.ToString();
            gameHandler.NewCardsReceived += OnNewCardsReceived;
            gameHandler.PlayerStartedGuess += OnPlayerStartedGuess;
            gameHandler.PlayerGuessReceived += OnPlayerGuessReceived;
            gameHandler.RoundTimedOut += OnRoundTimedOut;
            gameHandler.CardHintReceived += OnCardHintReceived;
            gameHandler.RoundStarted += OnRoundStarted;
        }

        public void DestributePendingCards()
        {
            if (pendingCards != null && pendingCards.Count > 0)
            {
                StartCoroutine(DestributeCardsIE());
            }
        }

        private void OnPlayerStartedGuess(ISETPlayer player, DateTimeOffset _)
        {
            if(player.IsMe)
                block.SetActive(false);
        }

        private void OnPlayerGuessReceived(ISETPlayer player, IEnumerable<CardData> guessedCards, bool isCorrect)
        {
            List<CardUI> cards = new();

            foreach (var cardUI in selectedCards)
                cardUI.Reset();

            selectedCards.Clear();

            foreach (var guessCard in guessedCards) 
            {
                var card = placedCards.FirstOrDefault(c => c.info.Equals(guessCard));
                cards.Add(card);

                if(isCorrect)
                    placedCards.Remove(card);
            }

            byte r = CardData.CheckSET(cards[0].info, cards[1].info, cards[2].info);
            var guessDialog = GuessResultDialog.Show(cards.ToArray(), CardData.CheckSET(cards[0].info, cards[1].info, cards[2].info));
            isShowingGuessResult = true;
            DOVirtual.DelayedCall(5, () =>
            {
                block.SetActive(true);
                guessDialog.Close();
                isShowingGuessResult = false;

                // The round timed out while the guessed cards were shown in the dialog
                if (pendingTimeoutCards != null)
                {
                    var timeoutCards = pendingTimeoutCards;
                    pendingTimeoutCards = null;
                    StartCoroutine(RemoveTimeoutCardsIE(timeoutCards));
                    return;
                }

                DestributePendingCards();
                CardDestributionEnded?.Invoke();
            });
        }

        private void OnRoundTimedOut(IEnumerable<CardData> removedCards)
        {
            // Removed cards can be the ones in the guess dialog, so wait until they are back on the table
            if (isShowingGuessResult)
            {
                pendingTimeoutCards = removedCards.ToList();
                return;
            }

            StartCoroutine(RemoveTimeoutCardsIE(removedCards.ToList()));
        }

        IEnumerator RemoveTimeoutCardsIE(List<CardData> removedCards)
        {
            List<CardUI> removedCardUIs = placedCards.Where(cardUI => removedCards.Contains(cardUI.info)).ToList();

            Sequence fadeSequence = DOTween.Sequence();
            foreach (var cardUI in removedCardUIs)
            {
                placedCards.Remove(cardUI);
                selectedCards.Remove(cardUI);

                if (cardUI.TryGetComponent(out CanvasGroup canvasGroup) == false)
                    canvasGroup = cardUI.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
                fadeSequence.Join(canvasGroup.DOFade(0f, timeoutFadeDuration));
            }

            yield return fadeSequence.WaitForCompletion();

            foreach (var cardUI in removedCardUIs)
            {
                // Detach first, since Destroy is deferred and distribution looks for empty card holders
                cardUI.transform.SetParent(cardContainer, false);
                Destroy(cardUI.gameObject);
            }

            if (pendingCards.Count > 0)
                yield return DestributeCardsIE();
            else
                CardDestributionEnded?.Invoke();
        }

        private void OnCardHintReceived(CardData hintCard)
        {
            var cardUI = placedCards.FirstOrDefault(cardUI => cardUI.info.Equals(hintCard));
            if (cardUI != null)
                cardUI.ToggleHint(true);
        }

        private void OnRoundStarted(DateTimeOffset _)
        {
            foreach (var cardUI in placedCards)
                cardUI.ToggleHint(false);
        }

        public void ToggleCardSelection(bool isOn)
        {
            block.SetActive(!isOn);
        }

        private void OnNewCardsReceived(IEnumerable<CardData> cards)
        {
            pendingCards.AddRange(cards);
        }

        IEnumerator DestributeCardsIE()
        {
            int cardHolderIndex = 0;
            int index = 0;

            foreach (var card in pendingCards)
            {
                while (cardHolders[cardHolderIndex].childCount > 0)
                    cardHolderIndex++;

                CardUI cardUI = Instantiate(cardUIPrefab, deckHolder.position, Quaternion.identity, cardHolders[cardHolderIndex]);
                cardUI.InitializeUI(card, cardHolders[cardHolderIndex]);
                cardUI.MoveBack();
                cardUI.Selected += OnCardSelected;
                cardUI.UnSelected += OnCardUnselected;
                placedCards.Add(cardUI);
                remained--;
                remainTxt.text = remained.ToString();
                index++;
                yield return new WaitForSeconds(0.2f);
            }
            pendingCards.Clear();
            CardDestributionEnded?.Invoke();
        }

        private void OnCardUnselected(CardUI cardUI)
        {
            selectedCards.Remove(cardUI);
        }

        private void OnCardSelected(CardUI cardUI)
        {
            selectedCards.Add(cardUI);

            if(selectedCards.Count == 3)
            {
                gameHandler.GuessCards(selectedCards.Select(x => x.info));
                block.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            gameHandler.NewCardsReceived -= OnNewCardsReceived;
            gameHandler.RoundTimedOut -= OnRoundTimedOut;
            gameHandler.CardHintReceived -= OnCardHintReceived;
            gameHandler.RoundStarted -= OnRoundStarted;
        }
    }
}
