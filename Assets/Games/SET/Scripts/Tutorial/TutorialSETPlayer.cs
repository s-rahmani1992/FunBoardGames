using FunBoardGames.Network;
using System;

namespace FunBoardGames.SET
{
    public class TutorialSETPlayer : ISETPlayer
    {
        public bool IsMe => true;
        public Profile Profile { get; }
        public bool IsReady => true;
        public int WrongScore { get; internal set; }
        public int CorrectScore { get; internal set; }
        public bool IsBusted => false;
        public int UsedHintCount { get; internal set; }
        public bool HasLeft => false;
        public bool? Vote => null;

#pragma warning disable CS0067 // Nothing changes for an offline player, the events only satisfy the interface
        public event Action<int, int> IndexChanged;
        public event Action LeftGame;
        public event Action<bool> ReadyStatusChanged;
#pragma warning restore CS0067

        public TutorialSETPlayer(Profile profile)
        {
            Profile = profile ?? new Profile { PlayerName = "You" };
        }
    }
}
