using FishNet.Managing;
using OnlineBoardGames;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OnlineBoardGames
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] NetworkManager gameNetworkManager;

#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] bool isEditorServer;
#endif

        void Start()
        {
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
