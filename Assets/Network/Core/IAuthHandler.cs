
namespace FunBoardGames.Network
{
    public interface IAuthHandler
    {
        event System.Action<Profile> LoginSuccess;
        event System.Action<string> LoginFailed;

        void Authenticate(string playerName);
    }
}