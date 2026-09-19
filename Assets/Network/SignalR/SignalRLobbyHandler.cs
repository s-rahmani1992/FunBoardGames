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
        BoardGameData requestedGame;

        public SignalRLobbyHandler(HubConnection connection)
        {
            unityContext = SynchronizationContext.Current;
            _connection = connection;

            _connection.On<JoinRoomResponseMessage>(LobbyMessageNames.JoinRoom, (roomMsg) =>
            {
                unityContext.Post(_ => OnJoinRoomReceived(roomMsg), null);
            });
        }

        void OnJoinRoomReceived(JoinRoomResponseMessage msg) 
        {
            switch (requestedGame.Type)
            {
                case BoardGame.SET:
                    List<SignalRSETPlayer> players = new();
                    foreach(var player in msg.JoinedPlayers)
                    {
                        SignalRSETPlayer SETPlayer = new(_connection, player);
                        players.Add(SETPlayer);
                    }

                    var setGame = new SignalRSETGameHandler(_connection, players);
                    JoinedGame?.Invoke(setGame, players);
                    break;
                case BoardGame.CantStop:
                    List<SignalRCantStopPlayer> csPlayers = new();
                    foreach(var player in msg.JoinedPlayers)
                    {
                        SignalRCantStopPlayer csPlayer = new(_connection, player);
                        csPlayers.Add(csPlayer);
                    }
                    var csGame = new SignalRCantStopGameHandler(_connection, csPlayers);
                    JoinedGame?.Invoke(csGame, csPlayers);
                    break;
            }
        }

        public void JointTestGame(BoardGame game)
        {
            _connection.InvokeAsync(LobbyMessageNames.JoinStraightGame, new JoinStraightGameRequestMessage() {
                Game = (BoardGameType)game,
            });
        }

        public void JoinGame(BoardGameData gameData)
        {
            requestedGame = gameData;
            _connection.InvokeAsync(LobbyMessageNames.JoinGame, new JoinGameRequestMessage() {
                GameId = gameData.Id,
            });
        }
    }
}
