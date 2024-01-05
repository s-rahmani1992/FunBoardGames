using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

namespace OnlineBoardGames
{
    public class GameNetworkManager : NetworkManager
    {
#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] bool isEditorServer;

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (isEditorServer)
                StartServer();
        }
#endif

        [field: SerializeField] public bool OverrideGame { get; private set; }
        [field: SerializeField] public BoardGame GameType { get; private set; }
        [field: SerializeField] public int PlayerCount { get; private set; }

        public static Guid TestGuid = new(10, 10, 10, new byte[]{1,2,3,4,5,6,7,8});

        
        public static new GameNetworkManager singleton { get; private set; }

        public bool IsServer
        {
            get
            {
#if UNITY_EDITOR
                return isEditorServer;
#elif UNITY_SERVER
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            if (singleton != null)
                DestroyImmediate(gameObject);
            else
                singleton = this;
        }

        #region Server Part

        LobbyManager lobbyManager;

        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartServer()
        {
            DebugStep.Log("NetworkManager.OnStartServer()");
            var lobby = Instantiate(spawnPrefabs[0]);
            NetworkServer.Spawn(lobby);
            NetworkServer.Spawn(Instantiate(spawnPrefabs[1]));
        }

        /// <summary>
        /// Called on the server when a new client connects.
        /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            DebugStep.Log($"NetworkManager.OnServerConnect({conn.connectionId})");
        }

        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            DebugStep.Log($"NetworkManager.OnServerReady({conn.connectionId})");
            base.OnServerReady(conn);
        }

        #endregion

        #region Client Part

        /// <summary>
        /// This is invoked when the client is started.
        /// </summary>
        public override void OnStartClient()
        {
            //NetworkClient.RegisterHandler<RoomListResponse>(OnGetRoomList);
            //NetworkClient.RegisterHandler<NotifyJoinRoom>(OnRoomClientCreated);
        }
        
        /// <summary>
        /// Called on clients when disconnected from a server.
        /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
        /// </summary>
        public override void OnClientDisconnect()
        {
            if (SceneManager.GetActiveScene().name != "Login") SceneManager.LoadScene("Login");
            base.OnClientDisconnect();
        }

        #endregion
    }
}