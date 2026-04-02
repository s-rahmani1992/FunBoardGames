using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRCantStopGameHandler : ICantStopGameHandler, IDisposable
    {
        public event Action<IBoardGamePlayer> PlayerJoined;
        public event Action<IBoardGamePlayer> PlayerLeft;
        public event Action AllPlayersReady;

        List<SignalRCantStopPlayer> playerList = new();

        HubConnection _connection;
        SynchronizationContext unityContext;
        List<IDisposable> connectionHooks = new();

        public SignalRCantStopGameHandler(HubConnection connection, IEnumerable<SignalRCantStopPlayer> players)
        {
            unityContext = SynchronizationContext.Current;
            _connection = connection;

            connectionHooks.Add(_connection.On<PlayerJoinRoomResponseMessage>(LobbyMessageNames.PlayerJoinRoom, (playerMsg) =>
            {
                unityContext.Post(_ => OnPlayerJoinedReceived(playerMsg), null);
            }));

            connectionHooks.Add(_connection.On<PlayerLeaveRoomResponseMessage>(LobbyMessageNames.PlayerLeave, (leaveMsg) =>
            {
                unityContext.Post(_ => OnPlayerLeft(leaveMsg), null);
            }));

            connectionHooks.Add(_connection.On(LobbyMessageNames.AllPlayersReady, () =>
            {
                unityContext.Post(_ => OnAllPlayerReady(), null);
            }));
        }

        private void OnPlayerJoinedReceived(PlayerJoinRoomResponseMessage playerMsg)
        {
            SignalRCantStopPlayer player = new(_connection, playerMsg.NewPlayer);
            playerList.Add(player);
            PlayerJoined?.Invoke(player);
        }

        private void OnPlayerLeft(PlayerLeaveRoomResponseMessage leaveMsg)
        {
            SignalRCantStopPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == leaveMsg.ConnectionId);
            player.InvokeLeave();
            PlayerLeft?.Invoke(player);
            playerList.Remove(player);
            player.Dispose();

            if (player.IsMe)
            {
                Dispose();
            }
        }

        private void OnAllPlayerReady()
        {
            AllPlayersReady?.Invoke();
        }

        public void Dispose()
        {
            foreach (var d in connectionHooks)
                d.Dispose();
        }

        public void LeaveGame()
        {
            _connection.InvokeAsync(LobbyMessageNames.PlayerLeave);
        }

        public void ReadyUp()
        {
            _connection.InvokeAsync(LobbyMessageNames.PlayerReady);
        }
    }
}
