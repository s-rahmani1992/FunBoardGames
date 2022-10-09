using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.SET {
    public class GuessResultDialog : BaseDialog
    {
        [SerializeField] RectTransform resultMarks;
        [SerializeField] Transform cardHolder;
        [SerializeField] Texture2D correct, wrong;
        CardUI[] selectedCards;
        byte guessResult;

        Vector2[] poses = new Vector2[]
        {
            new Vector2(-180, -970),
            new Vector2(180, -970),
            new Vector2(0, -1190)
        };

        protected override void Awake()
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            base.Awake();
        }

        public static void Show(CardUI[] cards, byte result)
        {
            DialogManager.Instance.SpawnDialog<GuessResultDialog>(DialogShowOptions.OverAll, (d) =>
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
                selectedCards[i].GetComponent<Canvas>().sortingOrder = 5;
                selectedCards[i].GetComponent<Canvas>().sortingLayerName = "Dialog";
                if (i != 2)
                    selectedCards[i].GetComponent<LineMove>().MoveInLine(poses[i], MoveMode.FixedTime, 0.5f);
                else
                    selectedCards[i].GetComponent<LineMove>().MoveInLine(poses[i], MoveMode.FixedTime, 0.5f, (b) =>
                    {
                        resultMarks.gameObject.SetActive(true);
                    });
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
                SETGameUIManager.Instance.ClearSelected();
            }
            else
            {
                SETGameUIManager.Instance.RemoveSelected();
            }
            base.Close();
        }
    }
}