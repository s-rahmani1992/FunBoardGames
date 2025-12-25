using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.SET;
using FunBoardGames.SET;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRSETGameHandler : ISETGameHandler, IDisposable
    {
        public event Action<IBoardGamePlayer> PlayerJoined;
        public event Action<IBoardGamePlayer> PlayerLeft;
        public event Action AllPlayersReady;
        public event Action<IEnumerable<CardData>> NewCardsReceived;

        List<SignalRSETPlayer> playerList = new();
        HubConnection _connection;
        SynchronizationContext unityContext;
        List<IDisposable> subscription = new();

        public IEnumerable<ISETPlayer> Players => playerList;

        public SignalRSETGameHandler(HubConnection connection, IEnumerable<SignalRSETPlayer> players)
        {
            playerList = new(players);
            unityContext = SynchronizationContext.Current;
            _connection = connection;

            subscription.Add(_connection.On<PlayerJoinRoomResponseMessage>(LobbyMessageNames.PlayerJoinRoom, (playerMsg) =>
            {
                unityContext.Post(_ => OnPlayerJoinedReceived(playerMsg), null);
            }));

            subscription.Add(_connection.On<PlayerLeaveRoomResponseMessage>(LobbyMessageNames.PlayerLeave, (leaveMsg) =>
            {
                unityContext.Post(_ => OnPlayerLeft(leaveMsg), null);
            }));

            subscription.Add(_connection.On(LobbyMessageNames.AllPlayersReady, () =>
            {
                unityContext.Post(_ => OnAllPlayerReady(), null);
            }));

            subscription.Add(_connection.On<DistributeNewCardsMessage>(SETGameMessageNames.DistributeCards, (cardMsg) =>
            {
                unityContext.Post(_ => OnNewCardsReceived(cardMsg), null);
            }));
        }

        private void OnNewCardsReceived(DistributeNewCardsMessage cardMsg)
        {
            NewCardsReceived?.Invoke(cardMsg.NewCards.Select(cardDTO => new CardData(cardDTO.Color, cardDTO.Shape, cardDTO.CountIndex, cardDTO.Shading)));
        }

        private void OnAllPlayerReady()
        {
            AllPlayersReady?.Invoke();
        }

        private void OnPlayerLeft(PlayerLeaveRoomResponseMessage leaveMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == leaveMsg.ConnectionId);
            player.InvokeLeave();
            PlayerLeft?.Invoke(player);
            playerList.Remove(player);
            player.Dispose();

            if (player.IsMe)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            foreach(var d in subscription)
                d.Dispose();
        }

        private void OnPlayerJoinedReceived(PlayerJoinRoomResponseMessage playerMsg)
        {
            SignalRSETPlayer player = new(_connection, playerMsg.NewPlayer);
            playerList.Add(player);
            PlayerJoined?.Invoke(player);
        }

        public void LeaveGame()
        {
            _connection.InvokeAsync(LobbyMessageNames.PlayerLeave);
        }

        public void ReadyUp()
        {
            _connection.InvokeAsync(LobbyMessageNames.PlayerReady);
        }

        public void SignalGameLoaded()
        {
            _connection.InvokeAsync(SETGameMessageNames.GameLoaded);
        }
    }
}