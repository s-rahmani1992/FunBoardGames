using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public class BoardGameData
    {
        public uint Id { get; private set; }
        public string Name { get; private set; }
        public BoardGame Type {  get; private set; }
        public int NumberofPlayers {  get; private set; }

        public BoardGameData(uint id, BoardGame type, string name = null, int numberofPlayers = 0)
        {
            Id = id;
            Type = type;
            Name = name;
            NumberofPlayers = numberofPlayers;
        }
    }

    public class SETGameData : BoardGameData
    {
        public float GuessTime {  get; private set; }

        public SETGameData(uint id, float guessTime, string name = null, int numberofPlayers = 0, int attributes = 0)
            : base(id, BoardGame.SET, name, numberofPlayers)
        {
            GuessTime = guessTime;
            AttributeCount = attributes;
        }

        public int AttributeCount { get; private set; }
    }

    public class CantStopGameData : BoardGameData
    {
        public CantStopBoardData BoardData { get; private set; }

        public CantStopGameData(uint id, CantStopBoardData boardData, string name = null, int numberofPlayers = 0)
            : base(id, BoardGame.CantStop, name, numberofPlayers)
        {
            BoardData = boardData;
        }
    }

    public class UserGameData
    {
        public List<BoardGameData> Games {  get; private set; }

        public UserGameData(List<BoardGameData> games)
        {
            Games = games;
        }
    }

    public interface IUserHandler
    {
        void GetUserData();

        event Action<UserGameData> UserGameDataReceived;
    }
}
