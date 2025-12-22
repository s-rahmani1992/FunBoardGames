using System;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRSETPlayer : IBoardGamePlayer
    {
        public bool IsMe => ConnectionId == UserProfile.ConnectionId;

        public string Name { get; private set; }

        public bool IsReady => false;

        public event Action<int, int> IndexChanged;
        public event Action LeftGame;
        public event Action<bool> ReadyStatusChanged;

        public string ConnectionId { get; private set; }

        public SignalRSETPlayer(string name, string connectionId)
        {
            Name = name;
            ConnectionId = connectionId;
        }

        internal void InvokeLeave()
        {
            LeftGame?.Invoke();
        }
    }
}