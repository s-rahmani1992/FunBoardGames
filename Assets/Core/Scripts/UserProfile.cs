using FunBoardGames.Network;
using UnityEngine;

namespace FunBoardGames
{
    public class UserProfile : ScriptableObject
    {
        IAuthHandler _authHandler;
        public string PlayerName => Profile?.PlayerName;

        public Profile Profile { get; private set; }

        public static string ConnectionId { get; private set; }
        
        public void Register(IAuthHandler authHandler)
        {
            _authHandler = authHandler;
            _authHandler.LoginSuccess += OnAuthReceived;
        }

        private void OnAuthReceived(SignInResponse profile)
        {
            Profile = profile.Profile;
            ConnectionId = profile.Profile.ConnectionId;
            PlayerPrefs.SetString("username", profile.Profile.PlayerName);
            PlayerPrefs.SetString("password", profile.Token);
        }

        private void OnDisable()
        {
            if (_authHandler != null)
            {
                _authHandler.LoginSuccess -= OnAuthReceived;
            }
        }
    }
}