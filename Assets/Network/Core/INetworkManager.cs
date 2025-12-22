
using System;

namespace FunBoardGames.Network
{
    public interface INetworkManager : IDisposable
    {
        IAuthHandler AuthHandler { get; }
        ILobbyHandler LobbyHandler { get; }

        event Action OnInitialized;
        void Initialize();
    }
}