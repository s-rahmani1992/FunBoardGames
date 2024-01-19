using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField] InputField roomNameIn;
        [SerializeField] RoomUI roomUI;
        [SerializeField] Transform content;
        [SerializeField] TMP_Dropdown dropDown;
        [SerializeField] TMP_Dropdown listDropDown;
        [SerializeField] RoomDialog roomDialog;

        LobbyManager lobbyManager;
        BoardGame selectedCreateGame;
        BoardGame selectedListGame;

        // Start is called before the first frame update
        void Start()
        {
            lobbyManager = FindObjectOfType<LobbyManager>();
            lobbyManager.RoomListReceived += OnRoomListReceived;
            dropDown.AddOptions(new System.Collections.Generic.List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(BoardGame.SET.ToString()),
                new TMP_Dropdown.OptionData(BoardGame.CantStop.ToString()),
            });
            dropDown.onValueChanged.AddListener((v) => selectedCreateGame = (BoardGame)v);
            listDropDown.AddOptions(new System.Collections.Generic.List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(BoardGame.SET.ToString()),
                new TMP_Dropdown.OptionData(BoardGame.CantStop.ToString()),
            });
            listDropDown.onValueChanged.AddListener((v) => selectedListGame = (BoardGame)v);
        }

        private void OnRoomListReceived(RoomData[] list)
        {
            foreach (Transform t in content)
                Destroy(t.gameObject);
            foreach (var r in list)
            {
                var room = Instantiate(roomUI, content);
                room.Initialize(r);
                room.JoinClicked += OnJoinClicked;
            }
        }

        private void OnJoinClicked(RoomUI r)
        {
            DialogManager.Instance.ShowDialog(roomDialog, DialogShowOptions.OverAll, (lobbyManager, r.roomData.GameType, r.roomData.Name, (int?)(r.roomData.Id)));
        }

        public void SendCreateRoom()
        {
            int? g = null;
            DialogManager.Instance.ShowDialog(roomDialog, DialogShowOptions.OverAll, (lobbyManager, selectedCreateGame, roomNameIn.text, g));
        }

        public void SendRoomListRequest()
        {
            lobbyManager.CmdGetRoomList(selectedListGame);
        }

        private void OnDestroy()
        {
            lobbyManager.RoomListReceived -= OnRoomListReceived;
        }
    }
}