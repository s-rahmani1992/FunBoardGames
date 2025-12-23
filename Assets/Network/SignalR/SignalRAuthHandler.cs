using Microsoft.AspNetCore.SignalR.Client;
using FunBoardGames.Network.SignalR.Shared;
using System;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRAuthHandler : IAuthHandler
    {
        public event Action<Profile> LoginSuccess;
        public event Action<string> LoginFailed;

        HubConnection _connection; 
        SynchronizationContext unityContext;

        public SignalRAuthHandler(HubConnection connection)
        {
            unityContext = SynchronizationContext.Current;
            _connection = connection;
            _connection.On<LoginResponseMessage>(AuthenticationMessageNames.Login, (loginMsg) =>
            {
                unityContext.Post(_ => OnLogin(loginMsg), null);
            });
        }

        private void OnLogin(LoginResponseMessage msg)
        {
            if (msg.Success)
            {
                LoginSuccess?.Invoke(new Profile()
                {
                    ConnectionId = msg.Profile.ConnectionId,
                    PlayerName = msg.Profile.PlayerName,
                });
            }
            else 
            {
                LoginFailed?.Invoke(msg.ErrorMessage);
            }
        }

        public void Authenticate(string playerName)
        {
            _connection.InvokeAsync(AuthenticationMessageNames.Login, new LoginRequestMessage()
            {
                PlayerName = playerName,
            });
        }
    }
}