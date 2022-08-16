using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

namespace OnlineBoardGames
{
    public class RoomUIManager : MonoBehaviour
    {
        public static RoomUIManager Instance { get; private set; }

        [SerializeField]
        RectTransform playersPanel;
        [SerializeField]
        Button readyBtn;
        [SerializeField]
        Text roomTxt, logTxt;

        Coroutine toast;
        
        void Awake(){
            Instance = this;
        }

        private IEnumerator Start(){
            var eventHandler = SingletonUIHandler.GetInstance<RoomUIEventHandler>();
            eventHandler.OnLocalPlayerReady += () => { readyBtn.interactable = false; };
            eventHandler.OnOtherPlayerJoined += (player) => { Log($"Player {player} Joined"); };
            eventHandler.OnOtherPlayerLeft += (player) => { Log($"Player {player} Left"); };
            eventHandler.OnAllPlayersReady += () => { logTxt.text = "Wait For Game to Load"; };
            eventHandler.OnBeginStatChanged += (stat) => { logTxt.text = (stat ? "" : "Not Enough Players. Wait For Others to join"); };
            //while (BoardGameNetworkManager.singleton.session == null)
            yield return null;
            //roomTxt.text = (FindObjectsOfType<Mirror.NetworkBehaviour>().OfType<IRoom>().ToArray())[0].RoomName;
        }

        private void OnDestroy(){
            Instance = null;
        }

        public RoomPlayerUI GetUI(int number){
            if (number < 1) return null; 
            return playersPanel.GetChild(number - 1).GetComponent<RoomPlayerUI>();
        }

        public void ReaveRoom(){
            //BoardGameNetworkManager.singleton.StopClient();
            Mirror.NetworkClient.Send(new LeaveRoomMessage { });
        }

        public void SendReady(){
            Mirror.NetworkClient.Send(new PlayerReadyMessage { });
        }

        void Log(string str){
            if (toast == null)
                toast = StartCoroutine(Toast(str));
            else{
                StopCoroutine(toast);
                toast = StartCoroutine(Toast(str));
            }
        }

        IEnumerator Toast(string str){
            string currentTxt = logTxt.text;
            logTxt.text = str;
            yield return new WaitForSeconds(3);
            logTxt.text = currentTxt;
            toast = null;
        }
    }
}
