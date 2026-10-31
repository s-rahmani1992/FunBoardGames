using FunBoardGames.Network;
using System;
using System.Collections.Generic;

namespace FunBoardGames.CantStop
{
    public class TutorialCantStopPlayer : ICantStopPlayer
    {
        readonly SortedDictionary<int, int> conePositions = new();

        public bool IsMe => true;
        public ProfileData Profile { get; }
        public bool IsReady => true;
        public PlayerColor ConeColor => PlayerColor.Red;
        public IDictionary<int, int> ConePositions => conePositions;
        public int Score { get; internal set; }
        public int TimeoutCount => 0;

#pragma warning disable CS0067 // Nothing changes for an offline player, the events only satisfy the interface
        public event Action<int, int> IndexChanged;
        public event Action LeftGame;
        public event Action<bool> ReadyStatusChanged;
#pragma warning restore CS0067

        public TutorialCantStopPlayer(ProfileData profile)
        {
            Profile = profile ?? new ProfileData { PlayerName = "You" };
        }
    }
}
