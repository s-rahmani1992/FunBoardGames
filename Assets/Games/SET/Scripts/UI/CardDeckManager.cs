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
        [SerializeField] TMPro.TMP_Text remainTxt;
        [SerializeField] CardUI cardUIPrefab;

        ISETGameHandler gameHandler;

        public event Action CardDestributionEnded;

        int remained = 81;

        private void Start()
        {
            remainTxt.text = remained.ToString();
        }

        public void RegisterGameHandler(ISETGameHandler gameHandler)
        {
            this.gameHandler = gameHandler;
            gameHandler.NewCardsReceived += OnNewCardsReceived;
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
                cardUI.InitializeUI(card);
                cardUI.MoveBack();
                remained--;
                remainTxt.text = remained.ToString();
                index++;
                yield return new WaitForSeconds(0.2f);
            }

            CardDestributionEnded?.Invoke();
        }

        private void OnDestroy()
        {
            gameHandler.NewCardsReceived -= OnNewCardsReceived;
        }
    }
}
