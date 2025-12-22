using DG.Tweening;
using FunBoardGames.Network;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class RoomDialog : BaseDialog, IDataDialog<(ILobbyHandler lobbyManager, BoardGame gameType, string roomName, int? roomId)>
    {
        [SerializeField] RectTransform playersPanel;
        [SerializeField] RoomPlayerUI roomPlyerUI;
        [SerializeField] Button readyBtn;
        [SerializeField] Button leaveBtn;
        [SerializeField] TMP_Text gameTxt;
        [SerializeField] TMP_Text roomTxt;
        [SerializeField] TMP_Text logTxt;
        [SerializeField] GameObject waitingObject;
        [SerializeField] SceneAssetMap sceneMap;

        Coroutine toast;
        ILobbyHandler lobbyManager;
        IGameHandler gameHandler;
        string roomName;
        BoardGame gameType;
        int? roomId;

        public override void Show()
        {
            base.Show();
            gameTxt.text = gameType.ToString();
            lobbyManager.JoinedGame += OnRoomGenerated;
            readyBtn.onClick.AddListener(OnReadyClicked);
            leaveBtn.onClick.AddListener(OnLeaveClicked);
            waitingObject.SetActive(true);

            if (roomId == null)
                lobbyManager.CreateRoom(gameType, roomName);
            else
                lobbyManager.JoinRoom(gameType, roomId.GetValueOrDefault());
        }

        private void OnRoomGenerated(IGameHandler handler, IEnumerable<IBoardGamePlayer> players)
        {
            gameHandler = handler;
            roomTxt.text = roomName;
            waitingObject.SetActive(false);

            foreach (var p in players)
                OnPlayerAdded(p);

            gameHandler.PlayerJoined += OnPlayerJoined;
            gameHandler.PlayerLeft += OnPlayerLeft;
            gameHandler.AllPlayersReady += OnAllPlayersReady;
        }

        public override void OnClose()
        {
            lobbyManager.JoinedGame -= OnRoomGenerated;

            if (gameHandler != null)
            {
                gameHandler.PlayerJoined -= OnPlayerJoined;
                gameHandler.PlayerLeft -= OnPlayerLeft;
                gameHandler.AllPlayersReady -= OnAllPlayersReady;
            }

            base.OnClose();
        }

        private void OnPlayerAdded(IBoardGamePlayer player)
        {
            RoomPlayerUI UIPlayer = Instantiate(roomPlyerUI, playersPanel);
            UIPlayer.SetPlayer(player);

            if (player.IsMe)
            {
                player.ReadyStatusChanged += OnReadyChanged;
            }
        }

        private void OnReadyChanged(bool ready)
        {
            readyBtn.interactable = !ready;
            leaveBtn.interactable = !ready;
        }

        private void OnAllPlayersReady()
        {
            logTxt.text = "Wait For Game to Load";
            DOVirtual.DelayedCall(1, () => SceneManager.LoadScene(sceneMap.GetScene(gameType)));
        }

        private void OnPlayerLeft(IBoardGamePlayer player)
        {
            if (!player.IsMe)
                Log($"{player.Name} Left the room");
            else
                Close();
        }

        private void OnPlayerJoined(IBoardGamePlayer player)
        {
            OnPlayerAdded(player);
            Log($"{(player.IsMe ? "you" : player.Name)} Joined the room");
        }

        public RoomPlayerUI GetUI(int number)
        {
            if (number < 1) return null;
            return playersPanel.GetChild(number - 1).GetComponent<RoomPlayerUI>();
        }

        void OnLeaveClicked()
        {
            waitingObject.SetActive(true);
            gameHandler.LeaveGame();
        }

        void OnReadyClicked()
        {
            gameHandler.ReadyUp();
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

        public void Initialize((ILobbyHandler lobbyManager, BoardGame gameType, string roomName, int? roomId) data)
        {
            lobbyManager = data.lobbyManager;
            gameType = data.gameType;
            roomId = data.roomId;
            roomName = data.roomName;
        }
    }
}
