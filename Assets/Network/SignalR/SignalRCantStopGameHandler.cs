using FunBoardGames.CantStop;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.CantStop;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRCantStopGameHandler : ICantStopGameHandler, IDisposable
    {
        static Color[] playerColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };

        public event Action<IBoardGamePlayer> PlayerJoined;
        public event Action<IBoardGamePlayer> PlayerLeft;
        public event Action AllPlayersReady;

        List<SignalRCantStopPlayer> playerList = new();

        int[] dices = new int[4];

        HubConnection _connection;
        SynchronizationContext unityContext;
        List<IDisposable> connectionHooks = new();

        public IEnumerable<ICantStopPlayer> Players => playerList;

        public event Action<CantStopBoardData, ICantStopPlayer> GameDataReceived;
        public event Action<int[]> DiceRolled;

        public SignalRCantStopGameHandler(HubConnection connection, IEnumerable<SignalRCantStopPlayer> players)
        {
            playerList = new(players);
            unityContext = SynchronizationContext.Current;
            _connection = connection;

            connectionHooks.Add(_connection.On<PlayerJoinRoomResponseMessage>(LobbyMessageNames.PlayerJoinRoom, (playerMsg) =>
            {
                unityContext.Post(_ => OnPlayerJoinedReceived(playerMsg), null);
            }));

            connectionHooks.Add(_connection.On<PlayerLeaveRoomResponseMessage>(LobbyMessageNames.PlayerLeave, (leaveMsg) =>
            {
                unityContext.Post(_ => OnPlayerLeft(leaveMsg), null);
            }));

            connectionHooks.Add(_connection.On(LobbyMessageNames.AllPlayersReady, () =>
            {
                unityContext.Post(_ => OnAllPlayerReady(), null);
            }));

            connectionHooks.Add(_connection.On<GameDataMessage>(CantStopGameMessageNames.SendGameData, (gameDataMsg) =>
            {
                unityContext.Post(_ => OnGameDataReceived(gameDataMsg), null);
            }));

            connectionHooks.Add(_connection.On<RollDiceMessage>(CantStopGameMessageNames.RollDice, (diceMsg) =>
            {
                unityContext.Post(_ => OnDiceReceived(diceMsg), null);
            }));
        }

        private void OnDiceReceived(RollDiceMessage diceMsg)
        {
            dices = diceMsg.diceValues;
            DiceRolled?.Invoke(dices);
        }

        private void OnGameDataReceived(GameDataMessage gameDataMsg)
        {
            int index = 0;
            foreach(var player in playerList)
            {
                player.SetPlayerColor(playerColors[index]);
                index++;
            }

            GameDataReceived?.Invoke(new CantStopBoardData(gameDataMsg.BoardData.Columns), playerList.FirstOrDefault(player => player.ConnectionId == gameDataMsg.StartPlayerConnectionId));
        }

        private void OnPlayerJoinedReceived(PlayerJoinRoomResponseMessage playerMsg)
        {
            SignalRCantStopPlayer player = new(_connection, playerMsg.NewPlayer);
            playerList.Add(player);
            PlayerJoined?.Invoke(player);
        }

        private void OnPlayerLeft(PlayerLeaveRoomResponseMessage leaveMsg)
        {
            SignalRCantStopPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == leaveMsg.ConnectionId);
            player.InvokeLeave();
            PlayerLeft?.Invoke(player);
            playerList.Remove(player);
            player.Dispose();

            if (player.IsMe)
            {
                Dispose();
            }
        }

        private void OnAllPlayerReady()
        {
            AllPlayersReady?.Invoke();
        }

        public void Dispose()
        {
            foreach (var d in connectionHooks)
                d.Dispose();
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
            _connection.InvokeAsync(CantStopGameMessageNames.GameLoaded);
        }

        public void RollDice()
        {
            _connection.InvokeAsync(CantStopGameMessageNames.RollDice);
        }
    }
}
