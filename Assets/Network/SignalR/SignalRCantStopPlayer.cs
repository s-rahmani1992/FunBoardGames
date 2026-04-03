using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using UnityEngine;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRCantStopPlayer : ICantStopPlayer, IDisposable
    {
        public bool IsMe => ConnectionId == UserProfile.ConnectionId;

        public string Name { get; private set; }

        public bool IsReady { get; private set; }

        public string ConnectionId { get; private set; }

        public Color PlayerColor { get; private set; }

        public event Action<int, int> IndexChanged;
        public event Action LeftGame;
        public event Action<bool> ReadyStatusChanged;

        HubConnection _connection;
        SynchronizationContext unityContext;

        IDisposable connectionHooks;

        public SignalRCantStopPlayer(HubConnection connection, PlayerInfoDTO playerInfo)
        {
            _connection = connection;
            unityContext = SynchronizationContext.Current;
            Name = playerInfo.UserProfile.PlayerName;
            ConnectionId = playerInfo.UserProfile.ConnectionId;
            IsReady = playerInfo.IsReady;

            connectionHooks = _connection.On<PlayerReadyResponseMessage>(LobbyMessageNames.PlayerReady, (readyMsg) =>
            {
                unityContext.Post(_ => OnPlayerReadyReceived(readyMsg), null);
            });
        }

        internal void InvokeLeave()
        {
            LeftGame?.Invoke();
        }

        private void OnPlayerReadyReceived(PlayerReadyResponseMessage readyMsg)
        {
            if (readyMsg.ConnectionId != ConnectionId)
                return;

            IsReady = true;
            ReadyStatusChanged?.Invoke(true);
        }

        public void SetPlayerColor(Color color)
        {
            PlayerColor = color;
        }

        public void Dispose()
        {
            connectionHooks?.Dispose();
        }
    }
}
