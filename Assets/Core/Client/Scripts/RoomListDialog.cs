using FunBoardGames.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class RoomListDialog : BaseDialog, IDataDialog<(BoardGame gameType, ILobbyHandler lobbyHandler)>
    {
        [SerializeField] TMP_Text gameTxt;
        [SerializeField] RoomUI roomPrefab;
        [SerializeField] Transform roomContent;
        [SerializeField] RoomDialog roomDialog;
        [SerializeField] Button refreshButon;
        [SerializeField] Button closeButon;
        [SerializeField] GameObject waitingObject;

        BoardGame gameType;
        ILobbyHandler lobbyHandler;

        public void Initialize((BoardGame gameType, ILobbyHandler lobbyHandler) roomData)
        {
            gameType = roomData.gameType;
            gameTxt.text = gameType.ToString();
            lobbyHandler = roomData.lobbyHandler;
            refreshButon.onClick.AddListener(SendRoomListRequest);
        }

        public override void Show()
        {
            base.Show();
            lobbyHandler.RoomListReceived += OnRoomListReceived;
            closeButon.onClick.AddListener(Close);
            SendRoomListRequest();
        }

        public override void OnClose()
        {
            lobbyHandler.RoomListReceived -= OnRoomListReceived;
            base.OnClose();
        }

        private void OnRoomListReceived(IEnumerable<RoomInfo> list)
        {
            foreach (Transform t in roomContent)
                Destroy(t.gameObject);
            foreach (var r in list)
            {
                var room = Instantiate(roomPrefab, roomContent);
                room.Initialize(r);
                room.JoinClicked += OnJoinClicked;
            }

            waitingObject.SetActive(false);
        }

        private void OnJoinClicked(RoomUI room)
        {
            DialogManager.Instance.ShowDialog(roomDialog, DialogShowOptions.Replace, (lobbyHandler, room.roomData.GameType, room.roomData.Name, (int?)(room.roomData.Id)));
        }

        private void SendRoomListRequest()
        {
            waitingObject.SetActive(true);
            lobbyHandler.GetRoomList(gameType);
        }
    } 
}
