
namespace FunBoardGames.Network
{
    public class SignUpData
    {
        public string PlayerName { get; set; }
        public string DeviceId { get; set; }
    }

    public class SignInData
    {
        public int UserId { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
    }

    public enum AuthenticationError
    {
        Unknown,
        InvalidCredentials,
        TooManyAccounts,
    }

    public class SignInResponse
    {
        public ProfileData Profile { get; set; }
        public string Token { get; set; }
    }

    public interface IAuthHandler
    {
        event System.Action<SignInResponse> LoginSuccess;
        event System.Action<AuthenticationError> LoginFailed;

        void SignUp(SignUpData signUpData);
        void SignIn(SignInData signInData);
    }
}