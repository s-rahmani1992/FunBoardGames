using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class RoomListDialog : BaseDialog, IDataDialog<BoardGame>
    {
        [SerializeField] TMP_Text gameTxt;
        [SerializeField] RoomUI roomPrefab;
        [SerializeField] Transform roomContent;
        [SerializeField] RoomDialog roomDialog;
        [SerializeField] Button refreshButon;
        [SerializeField] Button closeButon;
        [SerializeField] GameObject waitingObject;

        BoardGame game;
        LobbyManager lobby;

        public void Initialize(BoardGame game)
        {
            this.game = game;
            gameTxt.text = game.ToString();
            lobby = FindObjectOfType<LobbyManager>();
            refreshButon.onClick.AddListener(SendRoomListRequest);
        }

        public override void Show()
        {
            base.Show();
            lobby.RoomListReceived += OnRoomListReceived;
            closeButon.onClick.AddListener(Close);
            SendRoomListRequest();
        }

        public override void OnClose()
        {
            lobby.RoomListReceived -= OnRoomListReceived;
            base.OnClose();
        }

        private void OnRoomListReceived(RoomData[] list)
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
            DialogManager.Instance.ShowDialog(roomDialog, DialogShowOptions.Replace, (lobby, room.roomData.GameType, room.roomData.Name, (int?)(room.roomData.Id)));
        }

        private void SendRoomListRequest()
        {
            waitingObject.SetActive(true);
            lobby.CmdGetRoomList(game);
        }
    } 
}
