
using System.Collections.Generic;
using FunBoardGames.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FunBoardGames.Network
{
    public class NetworkSingleton : MonoBehaviour
    {
        const string PrefabPath = "NetworkSingleton";
        const string LoginSceneName = "Login";

        [SerializeField] MessageDialog messageDialog;

        static NetworkSingleton instance;
        INetworkManager _networkManager;
        bool _disconnectDialogShown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void CreateSingleton()
        {
            var prefab = Resources.Load<NetworkSingleton>(PrefabPath);
            instance = Instantiate(prefab);
            instance.name = "NetWorkSingleton-DDOL";
            DontDestroyOnLoad(instance.gameObject);
        }

        public static void SetNetworkManager(INetworkManager networkManager)
        {
            instance.Register(networkManager);
        }

        void Register(INetworkManager networkManager)
        {
            if (_networkManager != null)
                _networkManager.Disconnected -= OnDisconnected;

            _networkManager = networkManager;
            _networkManager.Disconnected += OnDisconnected;
        }

        void OnDisconnected()
        {
            if (_disconnectDialogShown || DialogManager.Instance == null)
                return;

            _disconnectDialogShown = true;

            var data = new MessageData("Disconnected", "The connection to the server was lost.", new List<ButtonData>
            {
                new ButtonData("Reload", OnReloadClicked),
                new ButtonData("Quit", OnQuitClicked)
            });
            DialogManager.Instance.ShowDialog(messageDialog, DialogShowOptions.OverAll, data);
        }

        void OnReloadClicked()
        {
            _disconnectDialogShown = false;
            SceneManager.LoadScene(LoginSceneName);
        }

        void OnQuitClicked()
        {
            Application.Quit();
        }

        private void OnDestroy()
        {
            if (instance != this || _networkManager == null)
                return;

            _networkManager.Disconnected -= OnDisconnected;
            _networkManager.Dispose();
        }

        public static INetworkManager NetworkManager => instance._networkManager;
    }
}
