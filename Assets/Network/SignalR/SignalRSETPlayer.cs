using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRSETPlayer : SignalRPlayer, ISETPlayer
    {
        public int WrongScore { get; private set; } = 0;

        public int CorrectScore { get; private set; } = 0;

        public bool IsBusted { get; private set; } = false;

        public int UsedHintCount { get; private set; } = 0;

        public SignalRSETPlayer(HubConnection connection, PlayerInfoDTO playerInfo)
            : base(connection)
        {
            Profile = new ProfileData()
            {
                UserId = playerInfo.UserProfile.UserId,
                PlayerName = playerInfo.UserProfile.PlayerName,
                Avatar = playerInfo.UserProfile.Avatar,
            };
        }

        internal void SetWrongScore(int score)
        {
            WrongScore = score;
        }

        internal void SetCorrectScore(int score)
        {
            CorrectScore = score;
        }

        internal bool SetBusted(bool isBusted)
        {
            bool justBusted = isBusted && IsBusted == false;
            IsBusted = isBusted;
            return justBusted;
        }

        internal void AddUsedHint()
        {
            UsedHintCount++;
        }
    }
}