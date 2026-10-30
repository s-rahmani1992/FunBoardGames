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
            NetworkSingleton.SetNetworkManager(signalRNetworkManager);
            SceneManager.LoadScene("Login");
        }
    }
}
