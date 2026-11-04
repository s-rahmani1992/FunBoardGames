
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface IBoardGamePlayer
    {
        bool IsMe { get;}
        ProfileData Profile { get; }
        bool HasLeft { get; }
        int Index { get; }
        event Action LeftGame;
    }

    public interface IGameHandler : IDisposable
    {
        void JoinGame(GameData gameData);
        void LeaveGame();

        event Action JoinFailed;
        event Action GameBegan;
        event Action LeftGame;
    }

    public interface IGameHandlerT<TPlayer> : IGameHandler where TPlayer : IBoardGamePlayer
    {
        event Action<IEnumerable<TPlayer>> JoinedGame;
        event Action<TPlayer> PlayerJoined;
        event Action<TPlayer> PlayerLeft;
    }
}
