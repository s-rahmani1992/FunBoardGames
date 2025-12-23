using FunBoardGames.Network;
using UnityEngine;

namespace FunBoardGames
{
    public class UserProfile : ScriptableObject
    {
        IAuthHandler _authHandler;
        public string PlayerName { get; private set; }

        public static string ConnectionId { get; private set; }
        
        public void Register(IAuthHandler authHandler)
        {
            _authHandler = authHandler;
            _authHandler.LoginSuccess += OnAuthReceived;
        }

        private void OnAuthReceived(Profile profile)
        {
            PlayerName = profile.PlayerName;
            ConnectionId = profile.ConnectionId;
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