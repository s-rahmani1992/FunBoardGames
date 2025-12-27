using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET {
    public class GuessResultDialog : BaseDialog
    {
        [SerializeField] RectTransform resultMarks;
        [SerializeField] Transform cardHolder;
        [SerializeField] Texture2D correct, wrong;
        [SerializeField] Transform[] cardHolders;

        CardUI[] selectedCards;
        byte guessResult;

        public event Action Closed;

        protected override void Awake()
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            base.Awake();
        }

        public static GuessResultDialog Show(CardUI[] cards, byte result)
        {
            return DialogManager.Instance.SpawnDialog<GuessResultDialog>(DialogShowOptions.OverAll, (d) =>
            {
                GuessResultDialog dialog = (GuessResultDialog)d;
                dialog.selectedCards = cards;
                dialog.guessResult = result;
            });
        }

        public override void Show()
        {
            base.Show();
            resultMarks.gameObject.SetActive(false);
            var g = CardData.Byte2Result(guessResult);

            for (int i = 0; i < 4; i++)
            {
                resultMarks.GetChild(i).localPosition = new Vector3((byte)g[i] * 170, -140 * i);
                resultMarks.GetChild(i).GetComponent<RawImage>().texture = (g[i] == TripleComparisonResult.NONE ? wrong : correct);
            }

            for (int i = 0; i < selectedCards.Length; i++)
            {
                selectedCards[i].transform.SetParent(cardHolders[i]);

                if (i != 2)
                    selectedCards[i].transform.DOLocalMove(Vector3.zero, 0.5f);
                else
                    selectedCards[i].transform.DOLocalMove(Vector3.zero, 0.5f).OnComplete(() => resultMarks.gameObject.SetActive(true));
            }
        }

        public override void Close()
        {
            bool isSet = CardData.IsSET(guessResult);
            if (!isSet)
            {
                foreach (var s in selectedCards)
                {
                    s.MoveBack();
                    s.Mark(false);
                }
            }
            else
            {
                foreach (var s in selectedCards)
                {
                    Destroy(s.gameObject);
                }
            }
            Closed?.Invoke();
            base.Close();
        }
    }
}