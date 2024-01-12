using UnityEngine.SceneManagement;
using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;

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

        [field: SyncVar(OnChange = nameof(OnIndexChanged))]
        public byte Index { get; private set; }

        [field: SyncVar(OnChange = nameof(OnReadyChanged))]
        public bool IsReady { get; private set; }

        protected virtual void OnReadyChanged(bool oldVal, bool newVal, bool _)
        {
            ReadyChanged?.Invoke(newVal);
        }

        protected virtual void OnIndexChanged(byte oldVal, byte newVal, bool _) 
        {
            IndexChanged?.Invoke(oldVal, newVal);
        }

        #endregion

        #region Server Part

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer()
        {
            DebugStep.Log($"NetworkBehaviour<{LocalConnection.ClientId}>.OnstartServer()");
            Name = (Owner.CustomData as AuthData).playerName;
        }

        [Server]
        public void SetIndex(int index)
        {
            Index = (byte)index;
        }

        [ServerRpc]
        public void CmdReady()
        {
            IsReady = true;
        }

        [ServerRpc]
        public void CmdGameReady()
        {
            GameReady?.Invoke();
        }

        #endregion

        #region Client Part

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