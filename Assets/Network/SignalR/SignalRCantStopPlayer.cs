using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using FunBoardGames.CantStop;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRCantStopPlayer : SignalRPlayer, ICantStopPlayer
    {

        public PlayerColor ConeColor { get; private set; }

        public int TimeoutCount { get; private set; }

        public IDictionary<int, int> ConePositions => conePositions;

        public int Score { get; private set; }

        SortedDictionary<int, int> conePositions = new();

        public SignalRCantStopPlayer(HubConnection connection, PlayerInfoDTO playerInfo)
            :base(connection)
        {
            Profile = new ProfileData()
            {
                UserId = playerInfo.UserProfile.UserId,
                PlayerName = playerInfo.UserProfile.PlayerName,
                Avatar = playerInfo.UserProfile.Avatar,
            };
        }

        public void SetPlayerColor(PlayerColor color)
        {
            ConeColor = color;
        }

        internal void UpdateCones(SortedDictionary<int, int> conePositions)
        {
            this.conePositions = new(conePositions);
        }

        internal void UpdateScore(int score)
        {
            Score = score;
        }

        internal void SetTimeout(int timeout)
        {
            TimeoutCount = timeout;
        }
    }
}
