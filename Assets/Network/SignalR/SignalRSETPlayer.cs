using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRSETPlayer : ISETPlayer, IDisposable
    {
        public bool IsMe => ConnectionId == UserProfile.ConnectionId;

        public string Name { get; private set; }

        public bool IsReady { get; private set; }

        public int WrongScore { get; private set; } = 0;

        public int CorrectScore { get; private set; } = 0;

        public event Action<int, int> IndexChanged;
        public event Action LeftGame;
        public event Action<bool> ReadyStatusChanged;

        public string ConnectionId { get; private set; }

        HubConnection _connection;
        SynchronizationContext unityContext;

        IDisposable connectionHooks;

        public SignalRSETPlayer(HubConnection connection, PlayerInfoDTO playerInfo)
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

        private void OnPlayerReadyReceived(PlayerReadyResponseMessage readyMsg)
        {
            if (readyMsg.ConnectionId != ConnectionId)
                return;

            IsReady = true;
            ReadyStatusChanged?.Invoke(true);
        }

        internal void InvokeLeave()
        {
            LeftGame?.Invoke();
        }

        internal void SetWrongScore(int score)
        {
            WrongScore = score;
        }

        internal void SetCorrectScore(int score)
        {
            CorrectScore = score;
        }

        public void Dispose()
        {
            connectionHooks?.Dispose();
        }
    }
}