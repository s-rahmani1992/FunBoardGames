using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames.Client
{
    public class RoomDialog : BaseDialog, IDataDialog<(LobbyManager lobbyManager, BoardGame gameType, string roomName, int? roomId)>
    {
        [SerializeField] RectTransform playersPanel;
        [SerializeField] RoomPlayerUI roomPlyerUI;
        [SerializeField] Button readyBtn;
        [SerializeField] Button leaveBtn;
        [SerializeField] TMP_Text gameTxt;
        [SerializeField] TMP_Text roomTxt;
        [SerializeField] TMP_Text logTxt;
        [SerializeField] GameObject waitingObject;

        RoomManager roomManager;
        Coroutine toast;
        LobbyManager lobbyManager;
        string roomName;
        BoardGame gameType;
        int? roomId;

        public override void Show()
        {
            base.Show();
            gameTxt.text = gameType.ToString();
            lobbyManager = FindObjectOfType<LobbyManager>();
            lobbyManager.JoinedRoom += OnRoomGenerated;
            readyBtn.onClick.AddListener(OnReadyClicked);
            leaveBtn.onClick.AddListener(OnLeaveClicked);
            waitingObject.SetActive(true);

            if (roomId == null)
                lobbyManager.CmdCreateRoom(gameType, roomName);
            else
                lobbyManager.CmdJoinRoom(gameType, roomId.Value);
        }

        public override void OnClose()
        {
            lobbyManager.JoinedRoom -= OnRoomGenerated;

            if (roomManager != null)
            {
                roomManager.Leave -= Close;
                roomManager.PlayerJoined -= OnPlayerJoined;
                roomManager.PlayerLeft -= OnPlayerLeft;
                roomManager.AllPlayersReady -= OnAllPlayersReady;
            }

            base.OnClose();
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
            waitingObject.SetActive(false);

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

        public RoomPlayerUI GetUI(int number)
        {
            if (number < 1) return null;
            return playersPanel.GetChild(number - 1).GetComponent<RoomPlayerUI>();
        }

        void OnLeaveClicked()
        {
            waitingObject.SetActive(true);
            lobbyManager.CmdLeaveRoom();
        }

        void OnReadyClicked()
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

        public void Initialize((LobbyManager lobbyManager, BoardGame gameType, string roomName, int? roomId) data)
        {
            lobbyManager = data.lobbyManager;
            gameType = data.gameType;
            roomId = data.roomId;
            roomName = data.roomName;
        }
    }
}
