using FunBoardGames.Network;
using UnityEngine;

namespace FunBoardGames
{
    public class UserProfile : ScriptableObject
    {
        IAuthHandler _authHandler;
        IUserHandler _userHandler;

        public string PlayerName => Profile?.PlayerName;

        public ProfileData Profile { get; private set; }

        public static int UserId { get; private set; }
        
        public void Register(IAuthHandler authHandler, IUserHandler userHandler)
        {
            _authHandler = authHandler;
            _userHandler = userHandler;
            _authHandler.LoginSuccess += OnAuthReceived;
            _userHandler.ProfileUpdated += OnProfileUpdated;
        }

        private void OnProfileUpdated(ProfileData profile)
        {
            Profile = profile;
        }

        private void OnAuthReceived(SignInResponse profile)
        {
            Profile = profile.Profile;
            UserId = profile.Profile.UserId;
            PlayerPrefs.SetInt("user-id", profile.Profile.UserId);
            PlayerPrefs.SetString("password", profile.Token);
        }

        private void OnDisable()
        {
            if (_authHandler != null)
            {
                _authHandler.LoginSuccess -= OnAuthReceived;
            }

            if (_userHandler != null)
            {
                _userHandler.ProfileUpdated -= OnProfileUpdated;
            }
        }
    }
}