using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using UnityEngine;

namespace FunBoardGames.Network.SignalR
{

    [CreateAssetMenu(fileName = "SignalRNetworkManager", menuName = "Scriptable Objects/SignalRNetworkManager")]
    public class SignalRNetworkManager : ScriptableObject, INetworkManager
    {
        SignalRAuthHandler _authHandler;
        SignalRLobbyHandler _lobbyHandler;
        HubConnection _connection;
        SynchronizationContext unityContext;
        string serverUrl = "http://localhost:5020/game";

        public IAuthHandler AuthHandler => _authHandler;
        public ILobbyHandler LobbyHandler => _lobbyHandler;

        public event Action OnInitialized;

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

            _connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Connection failed: " + task.Exception?.GetBaseException());
                }
                else
                {
                    unityContext.Post(_ =>
                    {
                        Debug.Log("Connected to SignalR server.");
                        _authHandler = new SignalRAuthHandler(_connection);
                        _lobbyHandler = new SignalRLobbyHandler(_connection);
                        OnInitialized?.Invoke();
                    }, null);
                }
            });
        }
    }
}