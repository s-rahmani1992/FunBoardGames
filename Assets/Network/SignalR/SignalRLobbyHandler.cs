using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public static class LobbyMessageNames
    {
        public const string CreateRoom = "CreateRoom";
        public const string JoinRoom = "JoinRoom";
        public const string PlayerJoinRoom = "PlayerJoinRoom";
        public const string GetRoomList = "GetRoomList";
        public const string PlayerLeave = "PlayerLeave";
    }

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

            _connection.On<JoinRoomResponseMsg>(LobbyMessageNames.JoinRoom, (roomMsg) =>
            {
                unityContext.Post(_ => OnJoinRoomReceived(roomMsg), null);
            });

            _connection.On<GetRoomListResponseMsg>(LobbyMessageNames.GetRoomList, (roomListMsg) =>
            {
                unityContext.Post(_ => OnRoomListReceived(roomListMsg), null);
            });
        }

        private void OnRoomListReceived(GetRoomListResponseMsg roomListMsg)
        {
            RoomListReceived?.Invoke(roomListMsg.Rooms);
        }

        public void CreateRoom(BoardGame game, string roomName)
        {
            _connection.InvokeAsync(LobbyMessageNames.CreateRoom, new CreateRoomRequestMsg() { 
                Game = game, 
                RoomName = roomName,
            });
        }

        public void GetRoomList(BoardGame gameType)
        {
            _connection.InvokeAsync(LobbyMessageNames.GetRoomList, new GetRoomListRequestMsg() { 
                Game = gameType, 
            });
        }

        public void JoinRoom(BoardGame game, int roomId)
        {
            _connection.InvokeAsync(LobbyMessageNames.JoinRoom, new JoinRoomRequestMsg() { 
                Game = game, 
                RoomId = roomId,
            });
        }

        void OnJoinRoomReceived(JoinRoomResponseMsg msg) 
        {
            switch (msg.Game)
            {
                case BoardGame.SET:
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
