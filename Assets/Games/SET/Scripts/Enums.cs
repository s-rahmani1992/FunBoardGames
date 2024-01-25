using System;

namespace FunBoardGames.SET
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
        Finish,
    }

    [Serializable]
    public enum VoteAnswer
    {
        None = 0,
        NO = 1,
        YES = 2
    }
}
