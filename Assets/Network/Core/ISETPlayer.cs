
namespace FunBoardGames.Network
{
    public interface ISETPlayer : IBoardGamePlayer
    {
        int WrongScore { get; }
        int CorrectScore { get; }
        bool? Vote { get; }
    }
}