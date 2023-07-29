using System;

namespace OnlineBoardGames.SET
{
    [Serializable]
    public enum SETGameState : byte
    {
        Start,
        Normal,
        Destribute,
        Guess,
        ProcessGuess,
        CardVote,
    }

    [Serializable]
    public enum VoteAnswer
    {
        None = 0,
        NO = 1,
        YES = 2
    }
}
