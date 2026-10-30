
using System;

namespace FunBoardGames.Network
{
    public interface INetworkManager : IDisposable
    {
        IAuthHandler AuthHandler { get; }
        ILobbyHandler LobbyHandler { get; }
        IUserHandler UserHandler { get; }

        event Action Connected;
        event Action Disconnected;
        event Action<string> ConnectionFailed;

        void Initialize(string serverUrl);
        void Disconnect();
    }
}