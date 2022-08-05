using System;
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
        Button guessBtn, cardBtn;
        [SerializeField]
        SETSessionNetworkManager sessionManager;

        
        List<CardUI> selected = new List<CardUI>(3);
        List<CardUI> placedCardUIs = new List<CardUI>(18);

        public Color[] colors;
        public Sprite[] cardShapes;

        

        [SerializeField]
        Transform resultMarks, resultPanel;
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

        private void Start(){
            GameUIEventManager.OnGameStateChanged.AddListener(RefreshBtns);
        }

        private void OnDestroy(){
            GameUIEventManager.OnGameStateChanged.RemoveListener(RefreshBtns);
        }


        public void PlaceCards(byte[] cardInfos, byte[] cardPoses){
            for (int i = 0; i < cardInfos.Length; i++){
                placedCardUIs.Add(pool.PullFromList(0, cardInfos[i], cardPoses[i], 0.2f * i).GetComponent<CardUI>());
            }
        }

        public void RefreshBtns(GameState state){
            guessBtn.interactable = cardBtn.interactable = (state == GameState.Normal);
            cardBlock.enabled = sessionManager.CanSelect;
        }

        public void AttemptGuess(){
            Mirror.NetworkClient.Send(new AttempSETGuess());
        }

        public void AlertGuess(){
            timer.StartCountdown(sessionManager.roomInfo.guessTime);
        }

        public void PopTimeout(string str){
            for (int i = 0; i < selected.Count; i++)
                selected[i].Mark(false);
            selected.Clear();
            MyUtils.DelayAction(() => { GameUIEventManager.OnCommonOrLocalStateEvent?.Invoke(UIStates.Clear); }, 3, this);
        }

        public void PopResult(byte result, GuessSETMessage msg, Mirror.NetworkIdentity player){
            GameUIEventManager.OnCommonOrLocalStateEvent?.Invoke(UIStates.Clear);
            bool isCorrect = CardData.IsSET(result);
            string p = null;
            if (!player.hasAuthority){
                p = player.GetComponent<SETNetworkPlayer>().playerName;
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
            var g = CardData.Byte2Result(result);
            for (int i = 0; i < 4; i++)
                resultMarks.GetChild(i).transform.localPosition = new Vector3((byte)g[i] * 170, -140 * i);
            
            StartCoroutine(DisplayResult(isCorrect, p));
        }

        IEnumerator DisplayResult(bool isSet, string playerName){
            timer.Stop();
            for (int i = 0; i < selected.Count; i++){
                selected[i].GetComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = 1;
                selected[i].GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerName = "L3";
                selected[i].GetComponent<LineMove>().MoveInLine(poses[i], MoveMode.FixedTime, 0.5f);
            }
            yield return new WaitForSeconds(0.5f);
            if(playerName != null){
                if (isSet)
                    GameUIEventManager.OnOtherStateEvent?.Invoke(UIStates.GuessRight, playerName);
                else
                    GameUIEventManager.OnOtherStateEvent?.Invoke(UIStates.GuessWrong, playerName);
            }
            else
                GameUIEventManager.OnCommonOrLocalStateEvent?.Invoke(isSet ? UIStates.GuessRight : UIStates.GuessWrong);
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
            GameUIEventManager.OnCommonOrLocalStateEvent?.Invoke(UIStates.Clear);
            selected.Clear();
        }
        public void SendCardRequest(){
            Mirror.NetworkClient.Send(new DestributeRequest { });
        }
    }
}
