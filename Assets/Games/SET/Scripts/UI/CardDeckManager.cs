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

        ISETGameHandler gameHandler;

        public event Action CardDestributionEnded;

        public HashSet<CardUI> selectedCards = new(3);
        List<CardUI> placedCards = new();

        int remained = 81;

        private void Start()
        {
            remainTxt.text = remained.ToString();
        }

        public void RegisterGameHandler(ISETGameHandler gameHandler)
        {
            this.gameHandler = gameHandler;
            gameHandler.NewCardsReceived += OnNewCardsReceived;
            gameHandler.PlayerStartedGuess += OnPlayerStartedGuess;
            gameHandler.PlayerGuessReceived += OnPlayerGuessReceived;
        }

        private void OnPlayerStartedGuess(ISETPlayer player)
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
            DOVirtual.DelayedCall(5, () =>
            {
                block.SetActive(true);
                guessDialog.Close();
                CardDestributionEnded?.Invoke();
            });
        }

        public void ToggleCardSelection(bool isOn)
        {
            block.SetActive(!isOn);
        }

        private void OnNewCardsReceived(IEnumerable<CardData> cards)
        {
            StartCoroutine(DestributeCardsIE(cards));
        }

        IEnumerator DestributeCardsIE(IEnumerable<CardData> cards)
        {
            int cardHolderIndex = 0;
            int index = 0;

            foreach (var card in cards)
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
        }
    }
}
