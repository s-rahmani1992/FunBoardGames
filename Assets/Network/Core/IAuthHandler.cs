
namespace FunBoardGames.Network
{
    public class SignUpData
    {
        public string PlayerName { get; set; }
        public string DeviceId { get; set; }
    }

    public class SignInData
    {
        public string PlayerName { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
    }

    public class SignInResponse
    {
        public Profile Profile { get; set; }
        public string Token { get; set; }
    }

    public interface IAuthHandler
    {
        event System.Action<SignInResponse> LoginSuccess;
        event System.Action<string> LoginFailed;

        void SignUp(SignUpData signUpData);
        void SignIn(SignInData signInData);
    }
}