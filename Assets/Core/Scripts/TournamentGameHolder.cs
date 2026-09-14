using System.Collections.Generic;
using FunBoardGames.Network;
using UnityEngine;

namespace FunBoardGames
{
    public class TournamentGameHolder : ScriptableObject
    {
        IUserHandler _userHandler;

        public List<BoardGameData> Games { get; private set; }

        public void Register(IUserHandler userHandler)
        {
            if (_userHandler != null)
                _userHandler.UserGameDataReceived -= OnUserGameDataReceived;

            _userHandler = userHandler;
            _userHandler.UserGameDataReceived += OnUserGameDataReceived;
        }

        private void OnUserGameDataReceived(UserGameData userGameData)
        {
            Games = userGameData.Games;
        }

        private void OnDisable()
        {
            if (_userHandler != null)
            {
                _userHandler.UserGameDataReceived -= OnUserGameDataReceived;
            }
        }
    }
}
