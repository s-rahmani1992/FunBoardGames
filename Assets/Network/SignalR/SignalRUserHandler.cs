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
        }

        private void OnUserDataReceived(GetUserDataResponseMessage response)
        {
            var games = response.Games
                .Select(ConvertGameDto)
                .ToList();

            UserGameDataReceived?.Invoke(new UserGameData(games));
        }

        private static BoardGameData ConvertGameDto(GameDTO gameDto)
        {
            string name = gameDto.Name;
            int playerCount = (int)gameDto.PlayerCount;

            return gameDto switch
            {
                SETGameDTO setGameDto => new SETGameData(setGameDto.Id, setGameDto.GuessTime, name, playerCount, setGameDto.AttributeCount, setGameDto.RoundTime, setGameDto.WrongLimit),
                CantStopGameDTO cantStopGameDto => new CantStopGameData(cantStopGameDto.Id, new CantStopBoardData(cantStopGameDto.BoardData), name, playerCount),
                _ => new BoardGameData(gameDto.Id, (BoardGame)(int)gameDto.GameType, name, playerCount),
            };
        }

        public event Action<UserGameData> UserGameDataReceived;

        public void GetUserData()
        {
            _connection.InvokeAsync(UserMessageNames.GetUserData);
        }
    }
}
