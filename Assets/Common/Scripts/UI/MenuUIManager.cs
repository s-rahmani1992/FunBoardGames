using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField]
        InputField roomNameIn;
        [SerializeField]
        ObjectPoolManager pool;
        [SerializeField]
        Transform content;
        [SerializeField] RoomRequestContainer roomContainer;

        GameNetworkManager networkManager;

        // Start is called before the first frame update
        void Start()
        {
            networkManager = FindObjectOfType<GameNetworkManager>();
            networkManager.RoomListReceived += OnRoomListReceived;
        }

        private void OnRoomListReceived(RoomData[] list)
        {
            while (content.childCount > 0)
                pool.Push2List(content.GetChild(0).gameObject);
            foreach (var r in list)
                pool.PullFromList(0, content, r);
        }

        public void SendCreateRoom()
        {
            roomContainer.SetParameters(true, roomNameIn.text, BoardGame.SET);
            SceneManager.LoadScene("Room");
        }

        public void SendRoomListRequest()
        {
            Mirror.NetworkClient.Send(new GetRoomListMessage { gameType = BoardGame.SET });
        }

        private void OnDestroy()
        {
            networkManager.RoomListReceived -= OnRoomListReceived;
        }
    }
}