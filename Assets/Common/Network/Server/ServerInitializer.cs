using FishNet.Managing.Server;
using FishNet.Transporting;
using UnityEngine;

namespace OnlineBoardGames
{
    public class ServerInitializer : MonoBehaviour
    {
        [SerializeField] ServerManager serverManager;
        [SerializeField] LobbyManager lobbyManager;

        private void Start()
        {
            serverManager.OnServerConnectionState += OnServerConnectionState;
        }

        private void OnServerConnectionState(ServerConnectionStateArgs e)
        {
            if (e.ConnectionState != LocalConnectionState.Started)
                return;

            DebugStep.Log($"server connection change: {e.ConnectionState}");
            var g = Instantiate(lobbyManager.gameObject);
            serverManager.Spawn(g.gameObject);
        }
    }
}
