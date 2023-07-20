using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class RoomUIManager : MonoBehaviour
    {
        public static RoomUIManager Instance { get; private set; }

        [SerializeField]
        RectTransform playersPanel;
        [SerializeField] RoomPlayerUI roomPlyerUI;
        [SerializeField]
        Button readyBtn;
        [SerializeField]
        Text roomTxt, logTxt;
        [SerializeField] RoomRequestContainer roomContainer;

        Coroutine toast;
        
        void Awake(){
            Instance = this;
        }

        private void Start()
        {
            roomContainer.RoomGenerated += OnRoomGenerated;
            roomContainer.PlayerAdded += OnPlayerAdded;

            if (roomContainer.IsCreate)
                Mirror.NetworkClient.Send(new CreateRoomMessage { reqName = roomContainer.RoomName, gameType = roomContainer.GameType });
            else
                Mirror.NetworkClient.Send(new JoinMatchMessage { matchID = roomContainer.MatchId });

            var eventHandler = SingletonUIHandler.GetInstance<RoomUIEventHandler>();
            eventHandler.OnLocalPlayerReady += () => { readyBtn.interactable = false; };
            eventHandler.OnOtherPlayerJoined += (player) => { Log($"Player {player} Joined"); };
            eventHandler.OnOtherPlayerLeft += (player) => { Log($"Player {player} Left"); };
            eventHandler.OnAllPlayersReady += () => { logTxt.text = "Wait For Game to Load"; };
            eventHandler.OnBeginStatChanged += (stat) => { logTxt.text = (stat ? "" : "Not Enough Players. Wait For Others to join"); };
        }

        private void OnPlayerAdded(BoardGamePlayer player)
        {
            RoomPlayerUI UIPlayer = Instantiate(roomPlyerUI, playersPanel);
            UIPlayer.SetPlayer(player);
        }

        private void OnRoomGenerated(BoardGameRoomManager room)
        {
            roomTxt.text = room.roomName;
        }

        private void OnDestroy(){
            Instance = null;
        }

        public RoomPlayerUI GetUI(int number){
            if (number < 1) return null; 
            return playersPanel.GetChild(number - 1).GetComponent<RoomPlayerUI>();
        }

        public void ReaveRoom(){
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
