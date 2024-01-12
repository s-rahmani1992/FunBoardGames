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

        public IEnumerable<BoardGamePlayer> Players => players;

        public bool IsAcceptingPlayer { get; private set; }
        public BoardGamePlayer LocalPlayer { get; private set; }
        
        public event Action<BoardGamePlayer> PlayerJoined;
        public event Action<BoardGamePlayer> PlayerLeft;
        public event Action Leave;
        public event Action AllPlayersReady;
        public event Action GameBegin;

#if UNITY_EDITOR
        public byte PlayerCount => DirectGameContainer.Instance.IsDirectGameActive ? (byte)DirectGameContainer.Instance.PlayerCount : (byte)Players.Count();
#else
        public byte PlayerCount => (byte)Players.Count();
#endif

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

            for (int i = 0; i < players.Count; i++)
                players[i].SetIndex(i + 1);
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

        public override void OnStartClient()
        {
            players.OnChange += OnPlayersChange;
        }

        private void OnPlayersChange(SyncListOperation op, int index, BoardGamePlayer oldItem, BoardGamePlayer newItem, bool asServer)
        {
            if (op == SyncListOperation.Add)
            {
                if (newItem.IsOwner)
                    LocalPlayer = newItem;

                PlayerJoined?.Invoke(newItem);
                return;
            }

            if (op == SyncListOperation.RemoveAt)
            {
                PlayerLeft?.Invoke(oldItem);
                return;
            }
        }

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

        #endregion
    }
}
