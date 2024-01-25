using UnityEngine;
using UnityEngine.SceneManagement;

namespace FunBoardGames
{
    public class MenuDirectGameController : MonoBehaviour
    {
        [SerializeField] DirectGameContainer directGameContainer;

        LobbyManager lobbyManager;
        RoomManager roomManager;

        void Start()
        {
            lobbyManager = FindObjectOfType<LobbyManager>();

            if (directGameContainer.IsDirectGameActive)
            {
                lobbyManager.JoinedRoom += OnJoinedRoom;
                lobbyManager.CmdJoinRoom(directGameContainer.Game, directGameContainer.TestRoomId);
            }
        }

        private void OnJoinedRoom(RoomManager room)
        {
            DontDestroyOnLoad(room.gameObject);
            roomManager = room;

            foreach (var p in roomManager.Players)
                OnPlayerJoined(p);

            room.PlayerJoined += OnPlayerJoined;
        }

        private void OnPlayerJoined(BoardGamePlayer player)
        {
            DontDestroyOnLoad(player.gameObject);

            if (player.IsOwner)
                SceneManager.LoadScene("Game");
        }

        private void OnDestroy()
        {
            lobbyManager.JoinedRoom -= OnJoinedRoom;

            if (roomManager != null)
                roomManager.PlayerJoined -= OnPlayerJoined;
        }
    }
}
