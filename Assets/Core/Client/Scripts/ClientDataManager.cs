using FishNet.Managing.Client;
using FishNet.Transporting;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace OnlineBoardGames.Client
{
    public class ClientDataManager : MonoBehaviour
    {
        [SerializeField] ClientManager networkClientManager;

        static ClientDataManager instance;

        public bool disconnectFlag;

        private void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }

            else
                DestroyImmediate(gameObject);
        }

        private void Start()
        {
            networkClientManager.RegisterBroadcast<AuthSyncMessage>(OnAuthSynced);
            networkClientManager.OnClientConnectionState += OnClientConnectionState;
        }

        private void OnAuthSynced(AuthSyncMessage message)
        {
            networkClientManager.Connection.CustomData = message.authData;
        }

        private void OnDestroy()
        {
            networkClientManager.UnregisterBroadcast<AuthSyncMessage>(OnAuthSynced);
        }

        private void OnClientConnectionState(ClientConnectionStateArgs e)
        {
            if (e.ConnectionState == LocalConnectionState.Stopped)
            {
                disconnectFlag = true;

                if (SceneManager.GetActiveScene().name != "Login")
                    SceneManager.LoadScene("Login");
            }
        }

        public static bool CheckDisconnectFlag()
        {
            if (instance.disconnectFlag)
            {
                instance.disconnectFlag = false;
                return true;
            }

            return false;
        }
    }
}
