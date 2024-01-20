using FishNet.Managing;
using FishNet.Managing.Client;
using System.Linq;
using TMPro;
using UnityEngine;

namespace OnlineBoardGames.Client
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField] TMP_Text playerNameText;
        [SerializeField] RoomUI roomUI;
        [SerializeField] Transform content;
        [SerializeField] RoomDialog roomDialog;

        LobbyManager lobbyManager;
        BoardGame selectedListGame;
        ClientManager clientManager;

        // Start is called before the first frame update
        void Start()
        {
            lobbyManager = FindObjectOfType<LobbyManager>();
            lobbyManager.RoomListReceived += OnRoomListReceived;
            clientManager = NetworkManager.Instances.ElementAt(0).ClientManager;
            playerNameText.text = (clientManager.Connection.CustomData as AuthData).playerName;
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