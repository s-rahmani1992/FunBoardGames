using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class RoomDialog : BaseDialog
    {
        [SerializeField] RectTransform playersPanel;
        [SerializeField] RoomPlayerUI roomPlyerUI;
        [SerializeField] Button readyBtn;
        [SerializeField] Button leaveBtn;
        [SerializeField] Text roomTxt;
        [SerializeField] Text logTxt;

        RoomManager roomManager;
        Coroutine toast;
        LobbyManager lobbyManager;
        string roomName;
        BoardGame gameType;
        int? roomId;

        public void Initialize(LobbyManager lobbyManager, BoardGame gameType, string roomName, int? roomId = null)
        {
            this.lobbyManager = lobbyManager;
            this.gameType = gameType;
            this.roomId = roomId;
            this.roomName = roomName;
        }

        public override void Show()
        {
            base.Show(); 
            lobbyManager = FindObjectOfType<LobbyManager>();
            lobbyManager.JoinedRoom += OnRoomGenerated;

            if (roomId == null)
                lobbyManager.CmdCreateRoom(gameType, roomName);
            else
                lobbyManager.CmdJoinRoom(gameType, roomId.Value);
        }

        public override void Close()
        {
            base.Close();
        }

        private void OnPlayerAdded(BoardGamePlayer player)
        {
            RoomPlayerUI UIPlayer = Instantiate(roomPlyerUI, playersPanel);
            UIPlayer.SetPlayer(player);
            DontDestroyOnLoad(player.gameObject);

            if (player.IsOwner)
            {
                player.ReadyChanged += OnReadyChanged;
            }
        }

        private void OnReadyChanged(bool ready)
        {
            readyBtn.interactable = !ready;
            leaveBtn.interactable = !ready;
        }

        private void OnRoomGenerated(RoomManager room)
        {
            roomManager = room;
            DontDestroyOnLoad(roomManager.gameObject);
            roomTxt.text = room.Name;

            foreach (var p in roomManager.Players)
                OnPlayerAdded(p);

            roomManager.PlayerJoined += OnPlayerJoined;
            roomManager.PlayerLeft += OnPlayerLeft;
            roomManager.Leave += Close;
            roomManager.AllPlayersReady += OnAllPlayersReady;
        }

        private void OnAllPlayersReady()
        {
            logTxt.text = "Wait For Game to Load";
            DOVirtual.DelayedCall(1, () => SceneManager.LoadScene(roomManager.GameScene));
        }

        private void OnPlayerLeft(BoardGamePlayer player)
        {
            if (!player.IsOwner)
                Log($"{player.Name} Left the room");
        }

        private void OnPlayerJoined(BoardGamePlayer player)
        {
            OnPlayerAdded(player);
            Log($"{(player.IsOwner ? "you" : player.Name)} Joined the room");
        }

        private void OnDestroy()
        {
            lobbyManager.JoinedRoom -= OnRoomGenerated;

            if (roomManager != null)
            {
                roomManager.Leave -= Close;
                roomManager.PlayerJoined -= OnPlayerJoined;
                roomManager.PlayerLeft -= OnPlayerLeft;
                roomManager.AllPlayersReady -= OnAllPlayersReady;
            }
        }

        public RoomPlayerUI GetUI(int number)
        {
            if (number < 1) return null;
            return playersPanel.GetChild(number - 1).GetComponent<RoomPlayerUI>();
        }

        public void ReaveRoom()
        {
            lobbyManager.CmdLeaveRoom();
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
