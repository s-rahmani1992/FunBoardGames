
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network
{
    public class GameData
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public BoardGame Type { get; internal set; }
        public int NumberofPlayers { get; internal set; }
    }

    public class SETGame : GameData
    {
        public int StartCardCount { get; internal set; }
        public int AttributeCount { get; internal set; }
        public int RoundTime { get; internal set; }
        public int WrongLimit { get; internal set; }
        public int? HintLimit { get; internal set; }
    }

    public class CantStopGame : GameData
    {
        public CantStopBoardData BoardData { get; internal set; }
        public int MarkerConeCount { get; internal set; }
        public int WinConeCount { get; internal set; }
        public int RoundTime { get; internal set; }
        public int TimeoutLimit { get; internal set; }
    }

    public class UserGameData
    {
        public List<GameData> Games {  get; internal set; }
    }

    public interface IUserHandler
    {
        void GetUserData();
        void ChangePlayerName(string newName);
        void ChangeAvatar(int newAvatarId);

        event Action<UserGameData> UserGameDataReceived;
        event Action<ProfileData> ProfileUpdated;
    }
}
