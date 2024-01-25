using FishNet.Managing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FunBoardGames
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] NetworkManager gameNetworkManager;
        [SerializeField] GamePrefabs prefabs;
        [SerializeField] DirectGameContainer directGameContainer;

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
