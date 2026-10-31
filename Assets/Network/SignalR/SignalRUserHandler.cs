using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRUserHandler : IUserHandler
    {
        HubConnection _connection;
        SynchronizationContext unityContext;

        public SignalRUserHandler(HubConnection connection)
        {
            unityContext = SynchronizationContext.Current;
            _connection = connection;

            _connection.On<GetUserDataResponseMessage>(UserMessageNames.GetUserData, (response) =>
            {
                unityContext.Post(_ => OnUserDataReceived(response), null);
            });

            _connection.On<Profile>(UserMessageNames.ProfileUpdate, (profile) =>
            {
                unityContext.Post(_ => ProfileUpdated?.Invoke(new ProfileData()
                {
                    UserId = profile.UserId,
                    PlayerName = profile.PlayerName,
                    Avatar = profile.Avatar,
                }), null);
            });
        }

        private void OnUserDataReceived(GetUserDataResponseMessage response)
        {
            var games = response.Games
                .Select(ConvertGameDto)
                .ToList();

            UserGameDataReceived?.Invoke(new UserGameData()
            {
                Games = games,
            });
        }

        private static GameData ConvertGameDto(BoardGameData gameDto)
        {
            return gameDto switch
            {
                SETGameData setGameDto => new SETGame()
                {
                    Id = gameDto.Id,
                    Name = gameDto.Name,
                    Type = BoardGame.SET,
                    NumberofPlayers = gameDto.PlayerCount,
                    StartCardCount = setGameDto.VisibleCardCount,
                    HintLimit = setGameDto.HintLimit,
                    RoundTime = setGameDto.RoundTime,
                    WrongLimit = setGameDto.WrongLimit,
                    AttributeCount = (setGameDto.ShapeAttribute != null ? 0 : 1) +
                                     (setGameDto.ColorAttribute != null ? 0 : 1) +
                                     (setGameDto.CountAttribute != null ? 0 : 1) +
                                     (setGameDto.ShadingAttribute != null ? 0 : 1)
                },
                CantStopGameData cantStopGameDto => new CantStopGame()
                {
                    Id = gameDto.Id,
                    Name = gameDto.Name,
                    Type = BoardGame.CantStop,
                    NumberofPlayers = gameDto.PlayerCount,
                    BoardData = new CantStopBoardData(cantStopGameDto.BoardData),
                    MarkerConeCount = cantStopGameDto.MarkerConeCount,
                    RoundTime = cantStopGameDto.RoundTime,
                    TimeoutLimit = cantStopGameDto.TimeoutLimit,
                    WinConeCount = cantStopGameDto.WinConeCount,
                },
                _ => new GameData(),
            };
        }

        public event Action<UserGameData> UserGameDataReceived;
        public event Action<ProfileData> ProfileUpdated;

        public void GetUserData()
        {
            _connection.InvokeAsync(UserMessageNames.GetUserData);
        }

        public void ChangePlayerName(string newName)
        {
            _connection.InvokeAsync(UserMessageNames.ChangePlayerName, newName);
        }

        public void ChangeAvatar(int newAvatarId)
        {
            _connection.InvokeAsync(UserMessageNames.ChangePlayerAvatar, newAvatarId);
        }
    }
}
