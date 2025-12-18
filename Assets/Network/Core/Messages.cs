
namespace FunBoardGames.Network
{
    public class LoginRequestMsg
    {
        public string PlayerName { get; set; } = string.Empty;
    }

    public class LoginResponseMsg
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
    }
}