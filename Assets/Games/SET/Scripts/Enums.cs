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
}
