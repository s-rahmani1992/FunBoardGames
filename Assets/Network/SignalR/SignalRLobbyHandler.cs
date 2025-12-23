using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRLobbyHandler : ILobbyHandler
    {
        public event Action<IGameHandler, IEnumerable<IBoardGamePlayer>> JoinedGame;
        public event Action<IEnumerable<RoomInfo>> RoomListReceived;

        HubConnection _connection;
        SynchronizationContext unityContext;

        public SignalRLobbyHandler(HubConnection connection)
        {
            unityContext = SynchronizationContext.Current;
            _connection = connection;

            _connection.On<JoinRoomResponseMessage>(LobbyMessageNames.JoinRoom, (roomMsg) =>
            {
                unityContext.Post(_ => OnJoinRoomReceived(roomMsg), null);
            });

            _connection.On<GetRoomListResponseMessage>(LobbyMessageNames.GetRoomList, (roomListMsg) =>
            {
                unityContext.Post(_ => OnRoomListReceived(roomListMsg), null);
            });
        }

        private void OnRoomListReceived(GetRoomListResponseMessage roomListMsg)
        {
            RoomListReceived?.Invoke(roomListMsg.Rooms.Select((roomDTO) => new RoomInfo{
                GameType = (BoardGame)roomDTO.GameType,
                Id = roomDTO.Id,
                MaxPlayers = roomDTO.MaxPlayers,
                Name = roomDTO.Name,
                PlayerCount = roomDTO.PlayerCount,
            }));
        }

        public void CreateRoom(BoardGame game, string roomName)
        {
            _connection.InvokeAsync(LobbyMessageNames.CreateRoom, new CreateRoomRequestMessage() { 
                Game = (BoardGameType)game, 
                RoomName = roomName,
            });
        }

        public void GetRoomList(BoardGame gameType)
        {
            _connection.InvokeAsync(LobbyMessageNames.GetRoomList, new GetRoomListRequestMessage() { 
                Game = (BoardGameType)gameType, 
            });
        }

        public void JoinRoom(BoardGame game, int roomId)
        {
            _connection.InvokeAsync(LobbyMessageNames.JoinRoom, new JoinRoomRequestMessage() { 
                Game = (BoardGameType)game, 
                RoomId = roomId,
            });
        }

        void OnJoinRoomReceived(JoinRoomResponseMessage msg) 
        {
            switch (msg.Game)
            {
                case BoardGameType.SET:
                    List<SignalRSETPlayer> players = new();
                    foreach(var player in msg.JoinedPlayers)
                    {
                        SignalRSETPlayer SETPlayer = new(player.PlayerName, player.ConnectionId);
                        players.Add(SETPlayer);
                    }

                    var setGame = new SignalRSETGameHandler(_connection, players);
                    JoinedGame?.Invoke(setGame, players);
                    break;
            }
        }
    }
}
