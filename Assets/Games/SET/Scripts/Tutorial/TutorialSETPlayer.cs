using FunBoardGames.Network;
using System;

namespace FunBoardGames.SET
{
    public class TutorialSETPlayer : ISETPlayer
    {
        public bool IsMe => true;
        public ProfileData Profile { get; }
        public int WrongScore { get; internal set; }
        public int CorrectScore { get; internal set; }
        public bool IsBusted => false;
        public int UsedHintCount { get; internal set; }
        public bool HasLeft => false;
        public int Index => 0;
        public bool? Vote => null;

#pragma warning disable CS0067 // Nothing changes for an offline player, the events only satisfy the interface
        public event Action LeftGame;
#pragma warning restore CS0067

        public TutorialSETPlayer(ProfileData profile)
        {
            Profile = profile ?? new ProfileData { PlayerName = "You" };
        }
    }
}
