using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRSETPlayer : ISETPlayer, IDisposable
    {
        public bool IsMe => UserId == UserProfile.UserId;

        public Profile Profile { get; private set; }

        public bool IsReady { get; private set; }

        public int WrongScore { get; private set; } = 0;

        public int CorrectScore { get; private set; } = 0;

        public bool IsBusted { get; private set; } = false;

        public int UsedHintCount { get; private set; } = 0;

        public bool HasLeft { get; private set; } = false;

        public event Action<int, int> IndexChanged;
        public event Action LeftGame;
        public event Action<bool> ReadyStatusChanged;

        public int UserId { get; private set; }

        public bool? Vote { get; private set; } = null;

        HubConnection _connection;
        SynchronizationContext unityContext;

        IDisposable connectionHooks;

        public SignalRSETPlayer(HubConnection connection, PlayerInfoDTO playerInfo)
        {
            _connection = connection;
            unityContext = SynchronizationContext.Current;
            Profile = new Profile()
            {
                UserId = playerInfo.UserProfile.UserId,
                PlayerName = playerInfo.UserProfile.PlayerName,
                Avatar = playerInfo.UserProfile.Avatar,
            };
            UserId = playerInfo.UserProfile.UserId;
        }

        internal void InvokeLeave()
        {
            HasLeft = true;
            LeftGame?.Invoke();
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

        internal void SetVote(bool? vote)
        {
            Vote = vote;
        }

        public void Dispose()
        {
            connectionHooks?.Dispose();
        }
    }
}