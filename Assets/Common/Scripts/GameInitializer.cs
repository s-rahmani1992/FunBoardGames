using OnlineBoardGames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] GameNetworkManager gameNetworkManager;

    void Start()
    {
        if (!gameNetworkManager.IsServer)
            SceneManager.LoadScene("Login");
    }
}
