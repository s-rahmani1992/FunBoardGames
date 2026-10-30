using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace FunBoardGames.Network.SignalR
{

    [CreateAssetMenu(fileName = "SignalRNetworkManager", menuName = "Scriptable Objects/SignalRNetworkManager")]
    public class SignalRNetworkManager : ScriptableObject, INetworkManager
    {
        [Header("Connection Settings")]
        [SerializeField] float reconnectTimeoutSeconds = 2f;
        [SerializeField] float reconnectIntervalSeconds = 1f;
        [SerializeField] float serverTimeoutSeconds = 6f;

        SignalRAuthHandler _authHandler;
        SignalRLobbyHandler _lobbyHandler;
        SignalRUserHandler _userHandler;
        HubConnection _connection;
        SynchronizationContext unityContext;

        public IAuthHandler AuthHandler => _authHandler;
        public ILobbyHandler LobbyHandler => _lobbyHandler;
        public IUserHandler UserHandler => _userHandler;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<string> ConnectionFailed;

        public void Dispose()
        {
            _connection?.DisposeAsync();
        }

        public void Initialize(string serverUrl)
        {
            unityContext = SynchronizationContext.Current;

            if (_connection != null)
            {
                _connection.Closed -= OnConnectionClosed;
                _connection.DisposeAsync();
                _connection = null;
            }

            var builder = new HubConnectionBuilder()
                .WithUrl(serverUrl)
                .WithAutomaticReconnect(BuildReconnectDelays());

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            builder.Services.AddSingleton<IHubProtocol>(new LogProtocal(new JsonHubProtocol()));
#endif
            try
            {
                _connection = builder.Build();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                ConnectionFailed?.Invoke(ex.Message);
                return;
            }

            _connection.ServerTimeout = TimeSpan.FromSeconds(serverTimeoutSeconds);
            _connection.KeepAliveInterval = TimeSpan.FromSeconds(serverTimeoutSeconds / 3f);

            _connection.Reconnecting += ex =>
            {
                Debug.Log("Connection lost, reconnecting... " + ex?.Message);
                return Task.CompletedTask;
            };
            _connection.Closed += OnConnectionClosed;

            _authHandler = new SignalRAuthHandler(_connection);
            _lobbyHandler = new SignalRLobbyHandler(_connection);
            _userHandler = new SignalRUserHandler(_connection);

            _connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    string error = task.Exception?.GetBaseException().Message;
                    Debug.LogError("Connection failed: " + error);
                    unityContext.Post(_ =>
                    {
                        ConnectionFailed?.Invoke(error);
                    }, null);
                }
                else
                {
                    unityContext.Post(_ =>
                    {
                        Debug.Log("Connected to SignalR server.");
                        Connected?.Invoke();
                    }, null);
                }
            });
        }

        TimeSpan[] BuildReconnectDelays()
        {
            float interval = Mathf.Max(0.1f, reconnectIntervalSeconds);
            int attempts = Mathf.CeilToInt(Mathf.Max(0f, reconnectTimeoutSeconds) / interval);
            var delays = new TimeSpan[attempts];
            for (int i = 0; i < attempts; i++)
                delays[i] = TimeSpan.FromSeconds(interval);
            return delays;
        }

        public void Disconnect()
        {
            _connection.StopAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Disconnection failed: " + task.Exception?.GetBaseException());
                }
            });
        }

        private Task OnConnectionClosed(Exception exception)
        {
            unityContext.Post(_ =>
            {
                Debug.Log("Disconnected from SignalR server." + (exception != null ? " Reason: " + exception.Message : ""));
                Disconnected?.Invoke();
            }, null);

            return Task.CompletedTask;
        }
    }
}