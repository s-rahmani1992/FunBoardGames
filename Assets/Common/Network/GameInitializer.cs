using FishNet.Managing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OnlineBoardGames
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] NetworkManager gameNetworkManager;
        [SerializeField] GamePrefabs prefabs;

#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] bool isEditorServer;
#endif

        void Start()
        {
            prefabs.Register(gameNetworkManager.SpawnablePrefabs);
            gameNetworkManager.ServerManager.SetStartOnHeadless(true);

#if !UNITY_EDITOR && !UNITY_SERVER
            SceneManager.LoadScene("Login");
#elif UNITY_EDITOR
            if (isEditorServer)
                gameNetworkManager.ServerManager.StartConnection();
            else
                SceneManager.LoadScene("Login");
#endif
        }
    }
}
