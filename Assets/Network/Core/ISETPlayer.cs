
namespace FunBoardGames.Network
{
    public interface ISETPlayer : IBoardGamePlayer
    {
        int WrongScore { get; }
        int CorrectScore { get; }
        bool IsBusted { get; }
        int UsedHintCount { get; }
        bool HasLeft { get; }
        bool? Vote { get; }
    }
}