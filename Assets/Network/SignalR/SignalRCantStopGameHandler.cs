using FunBoardGames.CantStop;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.CantStop;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRCantStopGameHandler : ICantStopGameHandler, IDisposable
    {
        const int NoDice = -1;

        static PlayerColor[] playerColors = new PlayerColor[] { PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue, PlayerColor.Yellow };

        public event Action<IBoardGamePlayer> PlayerJoined;
        public event Action<IBoardGamePlayer> PlayerLeft;
        public event Action AllPlayersReady;

        List<SignalRCantStopPlayer> playerList = new();

        int[] dices = new int[4];
        SortedDictionary<int, int> whiteConePositions = new();
        HashSet<int> playerFinishPositions = new();
        HubConnection _connection;
        SynchronizationContext unityContext;
        List<IDisposable> connectionHooks = new();

        public IEnumerable<ICantStopPlayer> Players => playerList;

        public IDictionary<int, int> WhiteConePositions => whiteConePositions;

        public IEnumerable<int> FinishedColumns => playerFinishPositions;

        public event Action<ICantStopPlayer> GameDataReceived;
        public event Action<int[], bool> DiceRolled;
        public event Action<ICantStopPlayer, IDictionary<int, int>, int, int> WhiteConesPlaced;
        public event Action<ICantStopPlayer, IDictionary<int, int>, int, int, ICantStopPlayer> RoundPlayed;
        public event Action<ICantStopPlayer, ICantStopPlayer> RoundCanceled;
        public event Action<IEnumerable<ICantStopPlayer>> GameFinished;
        public event Action ResetRoundReceived;
        public event Action BustSaverReceived;
        public event Action<ICantStopPlayer, ICantStopPlayer> TimedOut;
        public event Action<DateTimeOffset, ICantStopPlayer> RoundStarted;

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

            connectionHooks.Add(_connection.On<PlaceWhiteConeResponseMessage>(CantStopGameMessageNames.PlaceMarkerCone, (placeConesMsg) =>
            {
                unityContext.Post(_ => OnPlaceWhiteConesReceived(placeConesMsg), null);
            }));

            connectionHooks.Add(_connection.On<PlayRoundResponseMessage>(CantStopGameMessageNames.PlayRound, (playRoundMsg) =>
            {
                unityContext.Post(_ => OnPlayRoundReceived(playRoundMsg), null);
            }));

            connectionHooks.Add(_connection.On<EndRoundResponseMessage>(CantStopGameMessageNames.EndRound, (endRoundMsg) =>
            {
                unityContext.Post(_ => OnEndRoundReceived(endRoundMsg), null);
            }));

            connectionHooks.Add(_connection.On<GameFinishedResponseMessage>(CantStopGameMessageNames.GameFinished, (gameFinishedMsg) =>
            {
                unityContext.Post(_ => OnGameFinished(gameFinishedMsg), null);
            }));

            connectionHooks.Add(_connection.On(CantStopGameMessageNames.ResetRound, () =>
            {
                unityContext.Post(_ => ResetRoundReceived?.Invoke(), null);
            }));

            connectionHooks.Add(_connection.On(CantStopGameMessageNames.BustSaver, () =>
            {
                unityContext.Post(_ => BustSaverReceived?.Invoke(), null);
            }));

            connectionHooks.Add(_connection.On<TimeoutMessage>(CantStopGameMessageNames.Timeout, (timeoutMsg) =>
            {
                unityContext.Post(_ => OnTimeoutReceived(timeoutMsg), null);
            }));
        }

        private void OnGameFinished(GameFinishedResponseMessage gameFinishedMsg)
        {
            List<SignalRCantStopPlayer> finishedPlayers = new();

            gameFinishedMsg.PlayerScores.ToList().ForEach(scoreData =>
            {
                var player = playerList.FirstOrDefault(p => p.UserId == scoreData.UserId);
                if (player != null)
                {
                    player.UpdateScore(scoreData.Score);
                    finishedPlayers.Add(player);
                }
            });

            GameFinished?.Invoke(finishedPlayers);
        }

        private void OnEndRoundReceived(EndRoundResponseMessage endRoundMsg)
        {
            var player = playerList.FirstOrDefault(p => p.UserId == endRoundMsg.UserId);
            var nextPlayer = playerList.FirstOrDefault(p => p.UserId == endRoundMsg.NextPlayerUserId);

            if (endRoundMsg.IsBust)
            {
                whiteConePositions.Clear();
                RoundCanceled?.Invoke(player, nextPlayer);
                StartRoundTimer(endRoundMsg.StartTime, nextPlayer);
                return;
            }

            // The round was played after a saved bust, so there is no pair to place: the markers placed so far replace the player's cones
            player.UpdateCones(endRoundMsg.PlayerCones);
            player.UpdateScore(endRoundMsg.FinalScore);
            playerFinishPositions = new HashSet<int>(endRoundMsg.FinishedColumns);
            RoundPlayed?.Invoke(player, endRoundMsg.UpdatedColumns ?? new SortedDictionary<int, int>(), NoDice, NoDice, nextPlayer);
            StartRoundTimer(endRoundMsg.StartTime, nextPlayer);
            whiteConePositions.Clear();
        }

        private void OnPlayRoundReceived(PlayRoundResponseMessage playRoundMsg)
        {
            var player = playerList.FirstOrDefault(player => player.UserId == playRoundMsg.UserId);
            var nextPlayer = playerList.FirstOrDefault(player => player.UserId == playRoundMsg.NextPlayerUserId);
            
            foreach (var cone in playRoundMsg.UpdatedColumns)
                whiteConePositions[cone.Key] = cone.Value;

            player.UpdateCones(playRoundMsg.PlayerCones);
            player.UpdateScore(playRoundMsg.FinalScore);
            playerFinishPositions = new HashSet<int>(playRoundMsg.FinishedColumns);
            RoundPlayed?.Invoke(player, playRoundMsg.UpdatedColumns, playRoundMsg.DiceIndex1, playRoundMsg.DiceIndex2, nextPlayer);
            StartRoundTimer(playRoundMsg.StartTime, nextPlayer);
            whiteConePositions.Clear();
        }

        private void OnPlaceWhiteConesReceived(PlaceWhiteConeResponseMessage placeConesMsg)
        {
            var player = playerList.FirstOrDefault(player => player.UserId == placeConesMsg.UserId);
            
            foreach(var cone in placeConesMsg.UpdatedColumns)
                whiteConePositions[cone.Key] = cone.Value;

            WhiteConesPlaced?.Invoke(player, placeConesMsg.UpdatedColumns, placeConesMsg.DiceIndex1, placeConesMsg.DiceIndex2);
        }

        private void OnDiceReceived(RollDiceMessage diceMsg)
        {
            dices = diceMsg.diceValues;
            DiceRolled?.Invoke(dices, diceMsg.IsBusted);
            StartRoundTimer(diceMsg.StartTime, null);
        }

        private void OnGameDataReceived(GameDataMessage gameDataMsg)
        {
            int index = 0;
            foreach(var player in playerList)
            {
                player.SetPlayerColor(playerColors[index]);
                index++;
            }

            var startPlayer = playerList.FirstOrDefault(player => player.UserId == gameDataMsg.StartPlayerUserId);
            GameDataReceived?.Invoke(startPlayer);
            StartRoundTimer(gameDataMsg.StartTime, startPlayer);
        }

        // A message without a start time (the default value) did not start the server's round timer
        void StartRoundTimer(DateTimeOffset startTime, ICantStopPlayer player)
        {
            if (startTime.Year > 1)
                RoundStarted?.Invoke(startTime, player);
        }

        private void OnTimeoutReceived(TimeoutMessage timeoutMsg)
        {
            var player = playerList.FirstOrDefault(p => p.UserId == timeoutMsg.UserId);
            var nextPlayer = playerList.FirstOrDefault(p => p.UserId == timeoutMsg.NextUserId);
            whiteConePositions.Clear();
            TimedOut?.Invoke(player, nextPlayer);
            StartRoundTimer(timeoutMsg.RoundStartTime, nextPlayer);
        }

        private void OnPlayerJoinedReceived(PlayerJoinRoomResponseMessage playerMsg)
        {
            SignalRCantStopPlayer player = new(_connection, playerMsg.NewPlayer);
            playerList.Add(player);
            PlayerJoined?.Invoke(player);
        }

        private void OnPlayerLeft(PlayerLeaveRoomResponseMessage leaveMsg)
        {
            SignalRCantStopPlayer player = playerList.FirstOrDefault(player => player.UserId == leaveMsg.UserId);
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

            connectionHooks.Clear();
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

        public void PlaceWhiteCones(int diceIndex1, int diceIndex2, int? columnIndex1)
        {
            _connection.InvokeAsync(CantStopGameMessageNames.PlaceMarkerCone, new PlaceWhiteConeRequestMessage()
            {
                DiceIndex1 = diceIndex1,
                DiceIndex2 = diceIndex2,
                Selectedcolumn = columnIndex1,
            });
        }

        public void PlayRound(int diceIndex1, int diceIndex2, int? columnIndex1)
        {
            _connection.InvokeAsync(CantStopGameMessageNames.PlayRound, new PlaceWhiteConeRequestMessage()
            {
                DiceIndex1 = diceIndex1,
                DiceIndex2 = diceIndex2,
                Selectedcolumn = columnIndex1,
            });
        }

        public void EndRound()
        {
            _connection.InvokeAsync(CantStopGameMessageNames.EndRound);
        }

        public void ResetRound()
        {
            whiteConePositions.Clear();
            _connection.InvokeAsync(CantStopGameMessageNames.ResetRound);
        }

        public void UseBustSaver()
        {
            _connection.InvokeAsync(CantStopGameMessageNames.BustSaver);
        }
    }
}
