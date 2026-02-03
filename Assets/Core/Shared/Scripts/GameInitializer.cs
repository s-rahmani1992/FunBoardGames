using FunBoardGames.Network;
using FunBoardGames.Network.SignalR;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FunBoardGames
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] SignalRNetworkManager signalRNetworkManager;

        void Start()
        {
            Application.runInBackground = true;
            signalRNetworkManager.OnInitialized += OnNetworkInitialized;
            NetworkSingleton.SetNetworkManager(signalRNetworkManager);
        }

        private void OnDestroy()
        {
            signalRNetworkManager.OnInitialized -= OnNetworkInitialized;
        }

        private void OnNetworkInitialized()
        {
            SceneManager.LoadScene("Login");
        }
    }
}
