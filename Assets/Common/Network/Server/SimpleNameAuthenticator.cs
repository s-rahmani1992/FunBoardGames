using System;
using System.Collections.Concurrent;
using FishNet.Authenticating;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;

namespace OnlineBoardGames
{
    public class AuthData
    {
        public string playerName;
        public Guid roomID = Guid.Empty;
        public BoardGame gameType;
    }

    public class SimpleNameAuthenticator : Authenticator
    {
        public override event Action<NetworkConnection, bool> OnAuthenticationResult;

        readonly ConcurrentDictionary<string, NetworkConnection> playerNames = new();

        [Server]
        public override void InitializeOnce(NetworkManager networkManager)
        {
            base.InitializeOnce(networkManager);
            networkManager.ServerManager.RegisterBroadcast<AuthRequestMessage>(OnAuthRequestMessage, false);
            networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }

        [Server]
        void OnRemoteConnectionState(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs e)
        {
            if(e.ConnectionState == FishNet.Transporting.RemoteConnectionState.Stopped)
            {
                if(conn.CustomData != null)
                    playerNames.TryRemove((conn.CustomData as AuthData).playerName, out _);
            }
        }

        [Server]
        void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
        {
            AuthResponseMessage authResponseMessage = new();
            bool authenticated = false;

            if (msg.requestedName.Contains("%"))
            {
                authResponseMessage.resultCode = 1;
                authResponseMessage.message = "Player name cannot contain '%' character!";
            }
            else if (playerNames.TryAdd(msg.requestedName, conn))
            {
                conn.CustomData = new AuthData {playerName = msg.requestedName, roomID = Guid.Empty };
                authResponseMessage.resultCode = 0;
                authResponseMessage.message = "Authentication Success";
                authenticated = true;
            }
            else
            {
                authResponseMessage.resultCode = 1;
                authResponseMessage.message = $"Player with name '{msg.requestedName}' already exists!";
            }

            conn.Broadcast(authResponseMessage, false);
            OnAuthenticationResult?.Invoke(conn, authenticated);
        }
    }
}