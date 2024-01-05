using System.Collections.Generic;
using System;
using DG.Tweening;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Linq;

namespace OnlineBoardGames 
{
    public abstract class RoomManager : NetworkBehaviour
    {
        [field: SyncObject]
        readonly SyncList<BoardGamePlayer> players = new();

        public SyncList<BoardGamePlayer> Players => players;

        public bool IsAcceptingPlayer { get; private set; }
        public BoardGamePlayer LocalPlayer { get; private set; }
        
        public event Action<BoardGamePlayer> PlayerJoined;
        public event Action Leave;
        public event Action AllPlayersReady;
        public event Action GameBegin;

        public byte PlayerCount => (byte)Players.Count();

        //public byte PlayerCount => GameNetworkManager.singleton.OverrideGame ? (byte)GameNetworkManager.singleton.PlayerCount : (byte)Players.Count();
        public virtual BoardGame GameType { get; }

        int readyCount;
        int gameReadyCount;

        [Mirror.Scene]
        public string GameScene;

        [field: SyncVar]
        public string Name { get; private set; }

        [field: SyncVar]
        public int Id { get; private set; }

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

        [Server]
        public void AddPlayer(BoardGamePlayer player)
        {
            players.Add(player);

            player.ReadyChanged += (ready) =>
            {
                readyCount++;
                CheckAllReady();
            };

            player.GameReady += () =>
            {
                gameReadyCount++;
                CheckAllGameReady();
            };

            RPCPlayerJoin(player.GetComponent<NetworkObject>());

            for (int i = 0; i < players.Count; i++)
                players[i].SetIndex(i + 1);

            //UpdatePlayers(Players.ToList());
        }

        [Server]
        public void Remove(BoardGamePlayer player, bool canRefreshIndices = true)
        {
            if (players.Remove(player))
            {
                if (player.IsReady)
                    readyCount--;

                CheckAllReady();

                if (canRefreshIndices)
                {
                    for (int i = 0; i < PlayerCount; i++)
                        players[i].SetIndex(i + 1);
                }

                NetworkManager.ServerManager.Despawn(player.gameObject);
            }
        }

        [Server]
        void CheckAllReady()
        {
            if (PlayerCount >= 2 && readyCount == PlayerCount)
            {
                RPCAllPlayersReady();
                IsAcceptingPlayer = false;
            }
        }

        [Server]
        void CheckAllGameReady()
        {
            if (gameReadyCount == PlayerCount)
            {
                RPCSendGameReady();
                DOVirtual.DelayedCall(0.5f, BeginGame);
            }
        }

        public void SetParams(int id, string name)
        {
            Id = id;
            Name = name;
        }

        protected abstract void BeginGame();

        public override void OnStopClient()
        {
            Leave?.Invoke();
        }

        [ObserversRpc]
        protected void RPCSendGameReady() => GameBegin?.Invoke();

        #endregion

        #region Client Part

        [ObserversRpc]
        protected void UpdatePlayers(List<BoardGamePlayer> players)
        {
            foreach(var player in players)
            {
                if (players.Contains(player))
                    continue;
            }
        }

        [ObserversRpc]
        protected virtual void RPCAllPlayersReady()
        {
            AllPlayersReady?.Invoke();
        }

        [ObserversRpc]
        protected virtual void RPCPlayerJoin(NetworkObject identity)
        {
            if (identity.IsOwner)
                LocalPlayer = identity.GetComponent<BoardGamePlayer>();

            PlayerJoined?.Invoke(identity.GetComponent<BoardGamePlayer>());
        }

        #endregion
    }
}
