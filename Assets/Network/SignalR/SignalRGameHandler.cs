using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRPlayer : IBoardGamePlayer
    {
        public SignalRPlayer(HubConnection connection, PlayerInfoDTO playerInfo)
        {
            _connection = connection;
            _unityContext = SynchronizationContext.Current;
            Index = playerInfo.Index;
            Profile = new ProfileData()
            {
                UserId = playerInfo.UserProfile.UserId,
                PlayerName = playerInfo.UserProfile.PlayerName,
                Avatar = playerInfo.UserProfile.Avatar,
            };
        }

        public bool IsMe => Profile.UserId == UserProfile.UserId;
        public ProfileData Profile { get; protected set; }
        public int UserId => Profile.UserId;
        public int Index { get; }

        /// <summary>The player disconnected after the game began.</summary>
        public bool HasLeft{ get; private set; }

        public event Action LeftGame;

        protected readonly HubConnection _connection;
        protected readonly SynchronizationContext _unityContext;
        protected readonly List<IDisposable> _connectionHooks = new();

        public void InvokeLeave()
        {
            HasLeft = true;
            LeftGame?.Invoke();
        }

        public void Dispose()
        {
            foreach(var hook in _connectionHooks)
                hook.Dispose();

            _connectionHooks.Clear();
        }
    }

    // TInterface is the player type the game's handler interface exposes, e.g. ISETPlayer for ISETGameHandler
    public abstract class SignalRGameHandler<TPlayer, TInterface, TBegin> : IGameHandlerT<TInterface>
        where TPlayer : SignalRPlayer, TInterface
        where TInterface : IBoardGamePlayer
        where TBegin : class
    {
        public event Action JoinFailed;
        public event Action GameBegan;
        public event Action LeftGame;
        public event Action<IEnumerable<TInterface>> JoinedGame;
        public event Action<TInterface> PlayerJoined;
        public event Action<TInterface> PlayerLeft;
        protected readonly HubConnection connection;
        protected readonly SynchronizationContext unityContext;
        protected readonly List<TPlayer> playerList = new();
        protected readonly List<IDisposable> connectionHooks = new();

        protected SignalRGameHandler(HubConnection connection)
        {
            this.connection = connection;
            unityContext = SynchronizationContext.Current;

            connectionHooks.Add(connection.On<JoinRoomResponseMessage<PlayerInfoDTO, TBegin>>(LobbyMessageNames.JoinGame, msg =>
            {
                unityContext.Post(_ => OnJoinRoomReceived(msg), null);
            }));

            connectionHooks.Add(connection.On<PlayerJoinRoomResponseMessage<PlayerInfoDTO, TBegin>>(LobbyMessageNames.PlayerJoinGame, msg =>
            {
                unityContext.Post(_ => OnPlayerJoinRoomReceived(msg), null);
            }));

            connectionHooks.Add(connection.On<PlayerLeaveRoomResponseMessage>(LobbyMessageNames.PlayerLeave, msg =>
            {
                unityContext.Post(_ => OnPlayerLeaveReceived(msg), null);
            }));

            connectionHooks.Add(connection.On<JoinFailedMessage>(LobbyMessageNames.JoinFailed, msg =>
            {
                unityContext.Post(_ => JoinFailed?.Invoke(), null);
            }));
        }

        protected abstract TPlayer CreatePlayer(PlayerInfoDTO playerInfo);

        protected abstract void OnGameBegin(TBegin beginData);

        protected bool IsGameBegan { get; private set; }

        public void JoinGame(GameData gameData)
        {
            connection.InvokeAsync(LobbyMessageNames.JoinGame, new JoinGameRequestMessage()
            {
                GameId = gameData.Id,
            });
        }

        public void LeaveGame()
        {
            connection.InvokeAsync(LobbyMessageNames.PlayerLeave);
        }

        void OnJoinRoomReceived(JoinRoomResponseMessage<PlayerInfoDTO, TBegin> msg)
        {
            foreach (var playerInfo in msg.JoinedPlayers)
                AddPlayer(playerInfo);

            JoinedGame?.Invoke(playerList.Cast<TInterface>().ToList());
            BeginGameIfReady(msg.BeginData);
        }

        void OnPlayerJoinRoomReceived(PlayerJoinRoomResponseMessage<PlayerInfoDTO, TBegin> msg)
        {
            TPlayer player = AddPlayer(msg.NewPlayer);

            if (player != null)
                PlayerJoined?.Invoke(player);

            BeginGameIfReady(msg.BeginData);
        }

        TPlayer AddPlayer(PlayerInfoDTO playerInfo)
        {
            if (playerList.Any(p => p.Profile.UserId == playerInfo.UserProfile.UserId))
                return null;

            TPlayer player = CreatePlayer(playerInfo);
            playerList.Add(player);
            playerList.Sort((a, b) => a.Index.CompareTo(b.Index));
            return player;
        }

        void BeginGameIfReady(TBegin beginData)
        {
            if (beginData == null)
                return;

            IsGameBegan = true;
            OnGameBegin(beginData);
            GameBegan?.Invoke();
        }

        void OnPlayerLeaveReceived(PlayerLeaveRoomResponseMessage leaveMsg)
        {
            TPlayer player = playerList.FirstOrDefault(p => p.Profile.UserId == leaveMsg.UserId);

            if (player == null)
                return;

            player.InvokeLeave();
            PlayerLeft?.Invoke(player);

            // The player disconnected from a running game, so the player stays in the game and the results
            if (IsGameBegan)
                return;

            playerList.Remove(player);
            player.Dispose();

            if (player.IsMe)
            {
                LeftGame?.Invoke();
                Dispose();
            }
        }

        public virtual void Dispose()
        {
            foreach (var hook in connectionHooks)
                hook.Dispose();

            connectionHooks.Clear();
        }
    }
}
