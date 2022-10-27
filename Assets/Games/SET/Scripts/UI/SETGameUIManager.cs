using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.SET
{
    public class SETGameUIManager : MonoBehaviour
    {
        [SerializeField] ObjectPoolManager pool;
        [SerializeField] Button guessBtn, cardBtn, hintBtn;
        [SerializeField] Transform cardHolder;
        [SerializeField] Text remainTxt;
        [SerializeField] GraphicRaycaster cardRaycaster;

        public static SETGameUIManager Instance { get; private set; }

        public Transform playerPanel;
        public Timer timer;
        public Color[] colors;
        public Sprite[] cardShapes;

        int cardCount = 81;
        SETRoomNetworkManager sessionManager;
        List<CardUI> selected = new List<CardUI>(3);
        List<CardUI> hints = new List<CardUI>(3);
        List<CardUI> placedCardUIs = new List<CardUI>(18);

        public bool UpdateSelected(CardUI card)
        {
            if (!sessionManager.CanSelect || selected.Contains(card)) return false;
            if (selected.Count < 3)
            {
                selected.Add(card);
                if (selected.Count == 3 && sessionManager.state == SETGameState.Guess)
                    Mirror.NetworkClient.Send(new GuessSETMessage { card1 = selected[0].info, card2 = selected[1].info, card3 = selected[2].info });
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Awake()
        {
            Instance = this;
            sessionManager = FindObjectOfType<SETRoomNetworkManager>();
        }

        private void Start()
        {
            SingletonUIHandler.GetInstance<SETUIEventHandler>().OnGameStateChanged += RefreshBtns;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void UpdateCardMeter()
        {
            cardCount--;
            remainTxt.text = cardCount.ToString();
        }

        public void PlaceCards(CardData[] cardInfos, byte[] cardPoses)
        {
            for (int i = 0; i < cardInfos.Length; i++)
            {
                placedCardUIs.Add(pool.PullFromList(0, cardInfos[i], cardPoses[i], 0.2f * i, cardHolder).GetComponent<CardUI>());
            }
        }

        public void RefreshBtns(SETGameState state)
        {
            guessBtn.interactable = cardBtn.interactable = hintBtn.interactable = (state == SETGameState.Normal);
            for (int i = 0; i < hints.Count; i++)
                hints[i].Mark(false);
            hints.Clear();
            cardRaycaster.enabled = (state == SETGameState.Guess);
        }

        public void AttemptGuess()
        {
            sessionManager.CmdAttemptGuess(Mirror.NetworkClient.connection.identity);
        }

        public void AlertGuess()
        {
            timer.StartCountdown(sessionManager.roomInfo.guessTime);
        }

        public void PopTimeout(string str)
        {
            for (int i = 0; i < selected.Count; i++)
                selected[i].Mark(false);
            selected.Clear();
            MyUtils.DelayAction(() => { SingletonUIHandler.GetInstance<SETUIEventHandler>().OnCommonOrLocalStateEvent?.Invoke(UIStates.Clear); }, 3, this);
        }

        public void PopResult(byte result, GuessSETMessage msg, Mirror.NetworkIdentity player)
        {
            SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(UIStates.Clear);
            bool isCorrect = CardData.IsSET(result);
            string p = null;
            if (!player.hasAuthority)
            {
                p = player.GetComponent<SETNetworkPlayer>()?.playerName;
                selected.Clear();
                foreach (var c in placedCardUIs)
                {
                    if (c.info.Equals(msg.card1) || c.info.Equals(msg.card2) || c.info.Equals(msg.card3))
                    {
                        c.Mark(true);
                        selected.Add(c);
                    }
                    else
                        c.Mark(false);
                }
            }

            GuessResultDialog.Show(selected.ToArray(), result);
            timer.Stop();
            StartCoroutine(DisplayResult(isCorrect, p));
        }

        IEnumerator DisplayResult(bool isSet, string playerName)
        {
            yield return new WaitForSeconds(0.5f);
            if (playerName != null)
            {
                if (isSet)
                    SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnOtherStateEvent?.Invoke(UIStates.GuessRight, playerName);
                else
                    SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnOtherStateEvent?.Invoke(UIStates.GuessWrong, playerName);
            }
            else
                SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(isSet ? UIStates.GuessRight : UIStates.GuessWrong);
            
            yield return new WaitForSeconds(6);
            DialogManager.Instance.CloseDialog<GuessResultDialog>();
            SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(UIStates.Clear);
        }

        public void SendCardRequest()
        {
            sessionManager.CmdRequestDestribute(Mirror.NetworkClient.connection.identity);
        }

        public void SendHint()
        {
            sessionManager.CmdHintRequest(Mirror.NetworkClient.connection.identity);
        }

        public void RemoveSelected()
        {
            foreach (var s in selected)
            {
                pool.Push2List(s.gameObject);
                placedCardUIs.Remove(s);
            }
            selected.Clear();
        }

        public void ClearSelected()
        {
            selected.Clear();
        }

        public void MarkHints(CardData[] cards)
        {
            hints.Clear();
            if(cards.Length == 0) return;
            foreach (var c in placedCardUIs)
            {
                if (c.info.Equals(cards[0]) || c.info.Equals(cards[1]) || c.info.Equals(cards[2]))
                {
                    c.MarkHint();
                    hints.Add(c);
                }
            }
        }
    }
}
