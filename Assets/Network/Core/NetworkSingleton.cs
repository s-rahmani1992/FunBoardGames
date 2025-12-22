
using UnityEngine;

namespace FunBoardGames.Network
{
    public class NetworkSingleton : MonoBehaviour
    {
        static NetworkSingleton instance;
        INetworkManager _networkManager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void CreateSingleton()
        {
            GameObject gameObject = new GameObject("NetWorkSingleton-DDOL");
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<NetworkSingleton>();
        }

        public static void SetNetworkManager(INetworkManager networkManager)
        {
            instance._networkManager = networkManager;
            networkManager.Initialize();
        }

        private void OnDestroy()
        {
            instance._networkManager.Dispose();
        }

        public static INetworkManager NetworkManager => instance._networkManager;
    }
}
