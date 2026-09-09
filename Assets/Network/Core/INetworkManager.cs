
using System;

namespace FunBoardGames.Network
{
    public interface INetworkManager : IDisposable
    {
        IAuthHandler AuthHandler { get; }
        ILobbyHandler LobbyHandler { get; }

        event Action OnInitialized;
        event Action Connected;
        event Action Disconnected;
        event Action<string> ConnectionFailed;

        void Initialize();
        void Connect();
        void Disconnect();
    }
}