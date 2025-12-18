using FunBoardGames.Network;
using System;
using UnityEngine;

namespace FunBoardGames
{
    [CreateAssetMenu(fileName = "UserProfile", menuName = "Scriptable Objects/UserProfile")]
    public class UserProfile : ScriptableObject
    {
        IAuthHandler _authHandler;
        
        public void Register(IAuthHandler authHandler)
        {
            _authHandler = authHandler;
            _authHandler.OnAuthReceived += OnAuthReceived;
        }

        private void OnAuthReceived(LoginResponseMsg msg)
        {
            PlayerName = msg.PlayerName;
        }

        private void OnDisable()
        {
            if (_authHandler != null)
            {
                _authHandler.OnAuthReceived -= OnAuthReceived;
            }
        }

        public string PlayerName { get; private set; }
    }
}