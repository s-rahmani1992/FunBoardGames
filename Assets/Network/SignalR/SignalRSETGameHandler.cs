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
        public event Action GameStarted;
        public event Action<ISETPlayer, DateTimeOffset> PlayerStartedGuess;
        public event Action<ISETPlayer> PlayerGuessTimeout;
        public event Action<ISETPlayer> PlayerBusted;
        public event Action<ISETPlayer, IEnumerable<CardData>, bool> PlayerGuessReceived;
        public event Action<IEnumerable<ISETPlayer>> GameEnded;
        public event Action<DateTimeOffset> RoundStarted;
        public event Action<IEnumerable<CardData>> RoundTimedOut;

        List<SignalRSETPlayer> playerList = new();
        // Players who left are not in the server's final scores, but still appear at the end of the results
        List<SignalRSETPlayer> leftPlayers = new();
        HubConnection _connection;
        SynchronizationContext unityContext;
        List<IDisposable> connectionHooks = new();

        public IEnumerable<ISETPlayer> Players => playerList;

        public SignalRSETGameHandler(HubConnection connection, IEnumerable<SignalRSETPlayer> players)
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

            connectionHooks.Add(_connection.On<GameBeginMessage>(SETGameMessageNames.GameStarted, (gameMsg) =>
            {
                unityContext.Post(_ => OnGameBeginReceived(gameMsg), null);
            }));

            connectionHooks.Add(_connection.On<PlayerGuessStartMessage>(SETGameMessageNames.PlayerGuessStart, (guessMsg) =>
            {
                unityContext.Post(_ => OnPlayerStartedGuess(guessMsg), null);
            }));

            connectionHooks.Add(_connection.On<GuessResultResponse>(SETGameMessageNames.PlayerGuess, (guessMsg) =>
            {
                unityContext.Post(_ => OnPlayerGuessResultReceived(guessMsg), null);
            }));

            connectionHooks.Add(_connection.On<RoundTimeoutMessage>(SETGameMessageNames.RoundTimeout, (timeoutMsg) =>
            {
                unityContext.Post(_ => OnRoundTimeoutReceived(timeoutMsg), null);
            }));
        }

        private void OnGameBeginReceived(GameBeginMessage gameMsg)
        {
            OnNewCardsReceived(gameMsg.NewCards);
            GameStarted?.Invoke();
            RoundStarted?.Invoke(gameMsg.RoundStartTime);
        }

        private void OnRoundTimeoutReceived(RoundTimeoutMessage timeoutMsg)
        {
            if (timeoutMsg.NewCards != null)
                OnNewCardsReceived(timeoutMsg.NewCards);

            RoundTimedOut?.Invoke(timeoutMsg.RemovedCards.Select(ToCardData));

            if (timeoutMsg.RoundStartTime != null)
                RoundStarted?.Invoke(timeoutMsg.RoundStartTime.Value);

            if (timeoutMsg.FinalScores != null)
                OnFinalScoresReceived(timeoutMsg.FinalScores);
        }

        private void OnPlayerGuessResultReceived(GuessResultResponse guessMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == guessMsg.ConnectionId);

            bool justBusted = player.SetBusted(guessMsg.IsBusted);

            if (guessMsg.GuessedCards == null)
            {
                player.SetWrongScore(guessMsg.WrongScore);
                PlayerGuessTimeout?.Invoke(player);

                if (justBusted)
                    PlayerBusted?.Invoke(player);

                if (guessMsg.FinalScores != null)
                    OnFinalScoresReceived(guessMsg.FinalScores);
                return;
            }

            if(guessMsg.NewCards != null)
                OnNewCardsReceived(guessMsg.NewCards);

            player.SetCorrectScore(guessMsg.CorrectScore);
            player.SetWrongScore(guessMsg.WrongScore);
            PlayerGuessReceived?.Invoke(player, guessMsg.GuessedCards.Select(ToCardData), guessMsg.GuessedCorrect);

            if (justBusted)
                PlayerBusted?.Invoke(player);

            if (guessMsg.RoundStartTime != null)
                RoundStarted?.Invoke(guessMsg.RoundStartTime.Value);

            if(guessMsg.FinalScores != null)
                OnFinalScoresReceived(guessMsg.FinalScores);
        }

        private void OnFinalScoresReceived(List<SETPlayerResultDTO> finalScores)
        {
            List<SignalRSETPlayer> rankedPlayers = new();
            foreach (var playerScore in finalScores)
            {
                SignalRSETPlayer p = playerList.FirstOrDefault(p => p.ConnectionId == playerScore.ConnectionId);
                p.SetCorrectScore(playerScore.Corrects);
                p.SetWrongScore(playerScore.Wrongs);

                if (p.SetBusted(playerScore.IsBusted))
                    PlayerBusted?.Invoke(p);

                rankedPlayers.Add(p);
            }
            rankedPlayers.AddRange(leftPlayers);
            GameEnded?.Invoke(rankedPlayers);
            Dispose();
        }

        private void OnPlayerStartedGuess(PlayerGuessStartMessage guessMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == guessMsg.ConnectionId);
            PlayerStartedGuess?.Invoke(player, guessMsg.GuessStartTime);
        }

        private void OnNewCardsReceived(List<SETCardDTO> cardMsg)
        {
            NewCardsReceived?.Invoke(cardMsg.Select(ToCardData));
        }

        static CardData ToCardData(SETCardDTO cardDTO) => new(cardDTO.Color, cardDTO.Shape, cardDTO.CountIndex, cardDTO.Shading);

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
            leftPlayers.Add(player);
            player.Dispose();

            if (player.IsMe)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            foreach(var d in connectionHooks)
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
            _connection.InvokeAsync(SETGameMessageNames.GameStarted);
        }

        public void StartGuess()
        {
            _connection.InvokeAsync(SETGameMessageNames.PlayerGuessStart);
        }

        public void GuessCards(IEnumerable<CardData> cards)
        {
            _connection.InvokeAsync(SETGameMessageNames.PlayerGuess, new PlayerCardGuessRequest
            {
                GuessedCards = cards.Select(card => new SETCardDTO
                {
                    Color = card.Color,
                    Shading = card.Shading,
                    Shape = card.Shape,
                    CountIndex = card.CountIndex,
                }).ToList(),
            });
        }
    }
}