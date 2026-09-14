using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace FunBoardGames.Network.SignalR
{

    [CreateAssetMenu(fileName = "SignalRNetworkManager", menuName = "Scriptable Objects/SignalRNetworkManager")]
    public class SignalRNetworkManager : ScriptableObject, INetworkManager
    {
        SignalRAuthHandler _authHandler;
        SignalRLobbyHandler _lobbyHandler;
        SignalRUserHandler _userHandler;
        HubConnection _connection;
        SynchronizationContext unityContext;
        [SerializeField]
        [Tooltip("Editor/Standalone default: http://localhost:5020/game. " +
                 "On a physical Android device or emulator, 'localhost' means the device itself, not this PC. " +
                 "Override this per-build/per-test with your PC's LAN IP, e.g. http://192.168.1.23:5020/game " +
                 "(the exact host address depends on the emulator's network mode).")]
        string serverUrl = "http://localhost:5020/game";

        public IAuthHandler AuthHandler => _authHandler;
        public ILobbyHandler LobbyHandler => _lobbyHandler;
        public IUserHandler UserHandler => _userHandler;

        public event Action OnInitialized;
        public event Action Connected;
        public event Action Disconnected;
        public event Action<string> ConnectionFailed;

        public void Dispose()
        {
            _connection.DisposeAsync();
        }

        public void Initialize()
        {
            unityContext = SynchronizationContext.Current;

            _connection = new HubConnectionBuilder()
                .WithUrl(serverUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.Closed += OnConnectionClosed;

            _authHandler = new SignalRAuthHandler(_connection);
            _lobbyHandler = new SignalRLobbyHandler(_connection);
            _userHandler = new SignalRUserHandler(_connection);

            OnInitialized?.Invoke();
        }

        public void Connect()
        {
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