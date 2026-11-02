
using System;

namespace FunBoardGames.Network
{
    public interface INetworkManager : IDisposable
    {
        IAuthHandler AuthHandler { get; }
        IUserHandler UserHandler { get; }

        event Action Connected;
        event Action Disconnected;
        event Action<string> ConnectionFailed;
        event Action<string> UnexpectedError;

        void Initialize(string serverUrl);
        T CreateGameHandler<T>() where T : IGameHandler;
        void Disconnect();
    }
}