using UnityEngine.SceneManagement;
using Mirror;
using System;
using OnlineBoardGames.SET;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

namespace OnlineBoardGames
{
    public class BoardGamePlayer : NetworkBehaviour
    {
        public event Action<bool> ReadyChanged;
        public event Action<int, int> IndexChanged;
        public event Action LeftGame;
        public event Action GameReady;

        #region Syncvars

        [field: SyncVar]
        public string Name { get; private set; }

        [field: SyncVar(hook = nameof(OnIndexChanged))]
        public byte Index { get; private set; }

        [field: SyncVar(hook = nameof(OnReadyChanged))]
        public bool IsReady { get; private set; }

        protected virtual void OnReadyChanged(bool oldVal, bool newVal)
        {
            ReadyChanged?.Invoke(newVal);
        }

        protected virtual void OnIndexChanged(byte oldVal, byte newVal) 
        {
            IndexChanged?.Invoke(oldVal, newVal);
        }

        #endregion

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected virtual void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #region Server Part

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer()
        {
            DebugStep.Log($"NetworkBehaviour<{connectionToClient.connectionId}>.OnstartServer()");
            Name = (connectionToClient.authenticationData as AuthData).playerName;
        }

        [Server]
        public void SetIndex(int index)
        {
            Index = (byte)index;
        }

        [Command]
        public void CmdReady()
        {
            IsReady = true;
            ReadyChanged?.Invoke(true);
        }

        [Command]
        public void CmdGameReady()
        {
            GameReady?.Invoke();
        }

        #endregion

        #region Client Part

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient()
        {
            DebugStep.Log("BoardGamePlayer.OnStartClient()");
        }

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        public override void OnStopClient()
        {
            LeftGame?.Invoke();
        }
        void OnSceneLoaded(Scene scene1, LoadSceneMode mode)
        {
            if (scene1.name == "Room")
                OnRoomSceneLoaded();

            else if (scene1.name == "Game")
                OnGameSceneLoaded();
        }

        protected virtual void OnRoomSceneLoaded() { }
        protected virtual void OnGameSceneLoaded() { }
        
        #endregion
    }
}