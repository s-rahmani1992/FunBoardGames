using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.SET
{
    public class SETGameUIManager : MonoBehaviour
    {
        public Transform playerPanel;
        public Timer timer;
        [SerializeField]
        ObjectPoolManager pool;
        public Collider2D cardBlock;
        [SerializeField]
        Button guessBtn;
        [SerializeField]
        Text statTxt, timeOutTxt;
        [SerializeField]
        SETSessionNetworkManager sessionManager;

        
        List<CardUI> selected = new List<CardUI>(3);
        List<CardUI> placedCardUIs = new List<CardUI>(18);

        public Color[] colors;
        public Sprite[] cardShapes;

        

        [SerializeField]
        Transform resultMarks, resultPanel, timeoutPanel;
        Vector2[] poses = new Vector2[]{
            new Vector2(-1, -1.9f),
            new Vector2(1, -1.9f),
            new Vector2(0, -3)
        };

        public bool UpdateSelected(CardUI card){
            if (!sessionManager.CanSelect || selected.Contains(card)) return false;
            if (selected.Count < 3){
                selected.Add(card);
                if (selected.Count == 3 && sessionManager.state == GameState.Guess)
                    Mirror.NetworkClient.Send(new GuessSETMessage { card1 = selected[0].info.RawByte, card2 = selected[1].info.RawByte, card3 = selected[2].info.RawByte });
                return true;
            }
            else{
                return false;
            }
        }

        public void PlaceCards(byte[] cardInfos, byte[] cardPoses){
            for (int i = 0; i < cardInfos.Length; i++){
                placedCardUIs.Add(pool.PullFromList(0, cardInfos[i], cardPoses[i], 0.2f * i).GetComponent<CardUI>());
            }
        }

        public void RefreshBtns(GameState state){
            guessBtn.interactable = (state == GameState.Normal);
            cardBlock.enabled = sessionManager.CanSelect;
        }

        public void AttemptGuess(){
            Mirror.NetworkClient.Send(new AttempSETGuess());
        }

        public void AlertGuess(string GuessTxt){
            statTxt.text = GuessTxt;
            timer.StartCountdown(sessionManager.roomInfo.guessTime);
        }

        public void PopTimeout(string str){
            for (int i = 0; i < selected.Count; i++)
                selected[i].Mark(false);
            selected.Clear();
            timeOutTxt.text = str;
            timeoutPanel.gameObject.SetActive(true);
            statTxt.text = "";
            MyUtils.DelayAction(() => { timeoutPanel.gameObject.SetActive(false); }, 3, this);
        }

        public void PopResult(byte result, GuessSETMessage msg, Mirror.NetworkIdentity player){
            statTxt.gameObject.SetActive(false);
            bool isCorrect = MyUtils.IsSET(result);
            if (!player.hasAuthority){
                statTxt.text = (isCorrect ? $"{player.GetComponent<SETNetworkPlayer>().playerName} Guessed Right! He Got 1 point." : $"{player.GetComponent<SETNetworkPlayer>().playerName}'s Guess was Wrong! He lost 1 point.");
                    selected.Clear();
                foreach (var c in placedCardUIs){
                    if (c.info.RawByte == msg.card1 || c.info.RawByte == msg.card2 || c.info.RawByte == msg.card3){
                        c.Mark(true);
                        selected.Add(c);
                    }
                    else
                        c.Mark(false);
                }
            }
            else
                statTxt.text = (isCorrect ? "You Guessed Right! You Got 1 point." : "Your Guess was Wrong! You lost 1 point.");
            var g = MyUtils.Byte2Result(result);
            for (int i = 0; i < 4; i++)
                resultMarks.GetChild(i).transform.localPosition = new Vector3((byte)g[i] * 170, -140 * i);
            
            StartCoroutine(DisplayResult(MyUtils.IsSET(result)));
        }

        IEnumerator DisplayResult(bool isSet){
            timer.Stop();
            for (int i = 0; i < selected.Count; i++){
                selected[i].GetComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = 1;
                selected[i].GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerName = "L3";
                selected[i].GetComponent<LineMove>().MoveInLine(poses[i], MoveMode.FixedTime, 0.5f);
            }
            yield return new WaitForSeconds(0.5f);
            statTxt.gameObject.SetActive(true);
            resultPanel.gameObject.SetActive(true);
            yield return new WaitForSeconds(6);
            resultPanel.gameObject.SetActive(false);
            if (!isSet){
                foreach (var s in selected){
                    s.MoveBack();
                    s.Mark(false);
                }
            }
            else{
                foreach (var s in selected){
                    pool.Push2List(s.gameObject);
                    placedCardUIs.Remove(s);
                }
            }
            statTxt.text = "";
            selected.Clear();
        }
    }
}
