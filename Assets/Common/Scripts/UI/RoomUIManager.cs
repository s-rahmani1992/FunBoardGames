using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class RoomUIManager : MonoBehaviour
    {
        public static RoomUIManager Instance { get; private set; }

        [SerializeField] RectTransform playersPanel;
        [SerializeField] RoomPlayerUI roomPlyerUI;
        [SerializeField] Button readyBtn;
        [SerializeField] Button leaveBtn;
        [SerializeField] Text roomTxt;
        [SerializeField] Text logTxt;
        [SerializeField] RoomRequestContainer roomContainer;

        RoomManager roomManager;
        Coroutine toast;
        
        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GameNetworkManager.singleton.JoinedRoom += OnRoomGenerated;

            if (roomContainer.IsCreate)
                Mirror.NetworkClient.Send(new CreateRoomMessage { reqName = roomContainer.RoomName, gameType = roomContainer.GameType });
            else
                Mirror.NetworkClient.Send(new JoinMatchMessage { matchID = roomContainer.MatchId });
        }

        private void OnPlayerAdded(BoardGamePlayer player)
        {
            RoomPlayerUI UIPlayer = Instantiate(roomPlyerUI, playersPanel);
            UIPlayer.SetPlayer(player);

            if(player.hasAuthority)
                player.ReadyChanged += OnReadyChanged;
        }

        private void OnReadyChanged(bool ready)
        {
            readyBtn.interactable = !ready;
            leaveBtn.interactable = !ready;
        }

        private void OnRoomGenerated(RoomManager room)
        {
            roomManager = room;
            roomManager.PlayerAdded += OnPlayerAdded;
            roomManager.PlayerJoined += OnPlayerJoined;
            roomManager.PlayerLeft += OnPlayerLeft;
            roomManager.AllPlayersReady += OnAllPlayersReady;
            roomTxt.text = room.Name;
        }

        private void OnAllPlayersReady()
        {
            logTxt.text = "Wait For Game to Load";
            MyUtils.DelayAction(() => {
                SceneManager.LoadScene("Game");;
            }, 1, this);
        }

        private void OnPlayerLeft(BoardGamePlayer player)
        {
            if(!player.hasAuthority)
                Log($"{player.Name} Left the room");
        }

        private void OnPlayerJoined(BoardGamePlayer player)
        {
            Log($"{(player.hasAuthority ? "you" : player.Name)} Joined the room");
        }

        private void OnDestroy()
        {
            GameNetworkManager.singleton.JoinedRoom -= OnRoomGenerated;

            if(roomManager != null)
            {
                roomManager.PlayerAdded -= OnPlayerAdded;
                roomManager.PlayerJoined -= OnPlayerJoined; 
                roomManager.PlayerLeft -= OnPlayerLeft;
                roomManager.AllPlayersReady -= OnAllPlayersReady;
            }
            
            Instance = null;
        }

        public RoomPlayerUI GetUI(int number){
            if (number < 1) return null; 
            return playersPanel.GetChild(number - 1).GetComponent<RoomPlayerUI>();
        }

        public void ReaveRoom()
        {
            Mirror.NetworkClient.Send(new LeaveRoomMessage { });
        }

        public void SendReady()
        {
            roomManager.LocalPlayer.CmdReady();
        }

        void Log(string str)
        {
            if (toast == null)
                toast = StartCoroutine(Toast(str));
            else
            {
                StopCoroutine(toast);
                toast = StartCoroutine(Toast(str));
            }
        }

        IEnumerator Toast(string str)
        {
            string currentTxt = logTxt.text;
            logTxt.text = str;
            yield return new WaitForSeconds(3);
            logTxt.text = currentTxt;
            toast = null;
        }
    }
}
