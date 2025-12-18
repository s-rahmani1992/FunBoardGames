
namespace FunBoardGames.Network
{
    public static class NetworkSingleton
    {
        public static void SetNetworkManager(INetworkManager networkManager)
        {
            NetworkManager = networkManager;
            networkManager.Initialize();
        }

        public static INetworkManager NetworkManager { get; private set; }
    }
}
