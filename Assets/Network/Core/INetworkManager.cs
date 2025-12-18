
namespace FunBoardGames.Network
{
    public interface INetworkManager
    {
        event System.Action OnInitialized;
        void Initialize();

        IAuthHandler AuthHandler { get; }
    }
}