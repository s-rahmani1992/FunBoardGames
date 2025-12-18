using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using static UnityEngine.Audio.ProcessorInstance;

namespace FunBoardGames.Network.SignalR
{
    public static class MessageNames
    {
        public const string Login = "Login";
    }

    public class SignalRAuthHandler : IAuthHandler
    {
        HubConnection _connection; 
        SynchronizationContext unityContext;

        public SignalRAuthHandler(HubConnection connection)
        {
            unityContext = SynchronizationContext.Current;
            _connection = connection;
            _connection.On<LoginResponseMsg>(MessageNames.Login, OnLogin);
        }

        private void OnLogin(LoginResponseMsg msg)
        {
            unityContext.Post(_ => OnAuthReceived?.Invoke(msg), null);
        }

        public event Action<LoginResponseMsg> OnAuthReceived;

        public void Authenticate(LoginRequestMsg loginMsg)
        {
            _connection.InvokeAsync(MessageNames.Login, loginMsg);
        }
    }
}