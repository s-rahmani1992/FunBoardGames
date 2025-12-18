
namespace FunBoardGames.Network
{
    public interface IAuthHandler
    {
        void Authenticate(LoginRequestMsg loginMsg);

        event System.Action<LoginResponseMsg> OnAuthReceived;
    }
}