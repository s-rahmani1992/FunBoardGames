using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.
namespace OnlineBoardGames {
    public abstract class RoomManager : NetworkBehaviour
    {
        public List<BoardGamePlayer> Players { get; private set; } = new();
        public bool IsAcceptingPlayer { get; private set; }
        
        public event Action<BoardGamePlayer> PlayerAdded;
        public event Action<BoardGamePlayer> PlayerJoined;
        public event Action<BoardGamePlayer> PlayerLeft;
        public event Action AllPlayersReady;

        public byte PlayerCount => (byte)Players.Count;
        public virtual BoardGame GameType { get; }

        int readyCount;

        [field: SyncVar]
        public string Name { get; private set; }

        protected virtual void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        #region Server Part

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer()
        {
            IsAcceptingPlayer = true;
        }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient()
        {
            DebugStep.Log("NetworkRoomManager.OnStartClient()");
        }

        [Server]
        public void AddPlayer(BoardGamePlayer player)
        {
            Players.Add(player);
            RPCPlayerJoin(player.netIdentity);

            for (int i = 0; i < Players.Count; i++)
                Players[i].SetIndex(i + 1);

            UpdatePlayers(Players.ToArray());
        }

        [Server]
        public void Remove(BoardGamePlayer player, bool canRefreshIndices = true)
        {
            if (Players.Remove(player))
            {
                if (player.IsReady)
                    readyCount--;

                CheckAllReady();

                if (canRefreshIndices)
                {
                    for (int i = 0; i < PlayerCount; i++)
                        Players[i].SetIndex(i + 1);
                }

                NetworkServer.RemovePlayerForConnection(player.connectionToClient, true);
            }
        }

        [Server]
        void CheckAllReady()
        {
            if (PlayerCount >= 2 && readyCount == PlayerCount)
            {
                RPCAllPlayersReady();
                IsAcceptingPlayer = false;
                MyUtils.DelayAction(BeginGame, 1.3f, this);
            }
        }

        [Server]
        public void SetName(string name)
        {
            Name = name;
        }

        internal void OnPlayerReady(NetworkConnectionToClient conn, PlayerReadyMessage msg)
        {
            conn.identity.GetComponent<BoardGamePlayer>().SetReady();
            readyCount++;
            CheckAllReady();
        }

        protected abstract void BeginGame();

        #endregion

        #region Client Part

        [ClientRpc]
        protected void UpdatePlayers(BoardGamePlayer[] players)
        {
            foreach(var player in players)
            {
                if (Players.Contains(player))
                    continue;

                Players.Add(player);
                player.LeftGame += () => 
                {
                    PlayerLeft?.Invoke(player);
                    Players.Remove(player);
                };
                PlayerAdded?.Invoke(player);
            }
        }

        [ClientRpc]
        protected virtual void RPCAllPlayersReady()
        {
            AllPlayersReady?.Invoke();
        }

        [ClientRpc]
        protected virtual void RPCPlayerJoin(NetworkIdentity identity)
        {
            PlayerJoined?.Invoke(identity.GetComponent<BoardGamePlayer>());
        }

        #endregion
    }
}
