
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public interface IBoardGamePlayer
    {
        bool IsMe { get;}
        ProfileData Profile { get; }
        bool IsReady { get;}

        event Action<int, int> IndexChanged;
        event Action LeftGame;
        event Action<bool> ReadyStatusChanged;
    }

    public class RoomInfo
    {
        public uint Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BoardGame GameType { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
    }

    public interface IGameHandler : IDisposable
    {
        event Action<IBoardGamePlayer> PlayerJoined;
        event Action<IBoardGamePlayer> PlayerLeft;
        event Action AllPlayersReady;
        void LeaveGame();
        void ReadyUp();
    }

    public interface ILobbyHandler
    {
        void JointTestGame(BoardGame game);
        void JoinGame(BoardGameData gameData);

        event Action<IGameHandler, IEnumerable<IBoardGamePlayer>> JoinedGame;
    }
}