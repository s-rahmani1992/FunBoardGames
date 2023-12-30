using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField] InputField roomNameIn;
        [SerializeField] ObjectPoolManager pool;
        [SerializeField] Transform content;
        [SerializeField] TMP_Dropdown dropDown;
        [SerializeField] TMP_Dropdown listDropDown;
        [SerializeField] RoomRequestContainer roomContainer;

        GameNetworkManager networkManager;
        BoardGame selectedCreateGame;
        BoardGame selectedListGame;

        // Start is called before the first frame update
        void Start()
        {
            //networkManager = FindObjectOfType<GameNetworkManager>();
            //networkManager.RoomListReceived += OnRoomListReceived;
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
            //if (networkManager.OverrideGame)
            //{
            //    networkManager.JoinedRoom += OnJoinedRoom;
            //    NetworkClient.Send(new JoinMatchMessage { matchID = GameNetworkManager.TestGuid });
            //}
        }

        private void OnJoinedRoom(RoomManager room)
        {
            SceneManager.LoadScene(room.GameScene);
        }

        private void OnRoomListReceived(RoomData[] list)
        {
            while (content.childCount > 0)
                pool.Push2List(content.GetChild(0).gameObject);
            foreach (var r in list)
                pool.PullFromList(0, content, r, selectedListGame);
        }

        public void SendCreateRoom()
        {
            roomContainer.SetParameters(true, roomNameIn.text, selectedCreateGame);
            SceneManager.LoadScene("Room");
        }

        public void SendRoomListRequest()
        {
            NetworkClient.Send(new GetRoomListMessage { gameType = selectedListGame });
        }

        private void OnDestroy()
        {
            //networkManager.RoomListReceived -= OnRoomListReceived;
        }
    }
}