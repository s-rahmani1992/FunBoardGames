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
    public class SignalRSETGameHandler : SignalRGameHandler<SignalRSETPlayer, ISETPlayer, GameBeginMessage>, ISETGameHandler
    {
        public event Action<IEnumerable<CardData>> NewCardsReceived;
        public event Action GameStarted;
        public event Action<ISETPlayer, DateTimeOffset> PlayerStartedGuess;
        public event Action<ISETPlayer> PlayerGuessTimeout;
        public event Action<ISETPlayer> PlayerBusted;
        public event Action<ISETPlayer, IEnumerable<CardData>, bool> PlayerGuessReceived;
        public event Action<IEnumerable<ISETPlayer>> GameEnded;
        public event Action<DateTimeOffset> RoundStarted;
        public event Action<IEnumerable<CardData>> RoundTimedOut;
        public event Action<ISETPlayer> PlayerUsedHint;
        public event Action<CardData> CardHintReceived;

        // Players who left are not in the server's final scores, but still appear at the end of the results
        List<SignalRSETPlayer> leftPlayers = new();

        public IEnumerable<ISETPlayer> Players => playerList;

        public SignalRSETGameHandler(HubConnection connection) : base(connection)
        {
            connectionHooks.Add(connection.On<PlayerGuessStartMessage>(SETGameMessageNames.PlayerGuessStart, (guessMsg) =>
            {
                unityContext.Post(_ => OnPlayerStartedGuess(guessMsg), null);
            }));

            connectionHooks.Add(connection.On<GuessResultResponse>(SETGameMessageNames.PlayerGuess, (guessMsg) =>
            {
                unityContext.Post(_ => OnPlayerGuessResultReceived(guessMsg), null);
            }));

            connectionHooks.Add(connection.On<RoundTimeoutMessage>(SETGameMessageNames.RoundTimeout, (timeoutMsg) =>
            {
                unityContext.Post(_ => OnRoundTimeoutReceived(timeoutMsg), null);
            }));

            connectionHooks.Add(connection.On<CardHintResponse>(SETGameMessageNames.CardHint, (hintMsg) =>
            {
                unityContext.Post(_ => OnCardHintReceived(hintMsg), null);
            }));

            connectionHooks.Add(connection.On<PlayerUsedHintMessage>(SETGameMessageNames.PlayerUsedHint, (hintMsg) =>
            {
                unityContext.Post(_ => OnPlayerUsedHintReceived(hintMsg), null);
            }));
        }

        protected override SignalRSETPlayer CreatePlayer(PlayerInfoDTO playerInfo) => new(connection, playerInfo);

        protected override void OnPlayerRemoved(SignalRSETPlayer player) => leftPlayers.Add(player);

        protected override void OnGameBegin(GameBeginMessage gameMsg)
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
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.UserId == guessMsg.UserId);

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
                SignalRSETPlayer p = playerList.FirstOrDefault(p => p.UserId == playerScore.UserId);
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

        private void OnCardHintReceived(CardHintResponse hintMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.IsMe);
            player.AddUsedHint();
            PlayerUsedHint?.Invoke(player);
            CardHintReceived?.Invoke(ToCardData(hintMsg.Card));
        }

        private void OnPlayerUsedHintReceived(PlayerUsedHintMessage hintMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.UserId == hintMsg.UserId);
            if (player == null)
                return;

            player.AddUsedHint();
            PlayerUsedHint?.Invoke(player);
        }

        private void OnPlayerStartedGuess(PlayerGuessStartMessage guessMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.UserId == guessMsg.UserId);
            PlayerStartedGuess?.Invoke(player, guessMsg.GuessStartTime);
        }

        private void OnNewCardsReceived(List<SETCardDTO> cardMsg)
        {
            NewCardsReceived?.Invoke(cardMsg.Select(ToCardData));
        }

        static CardData ToCardData(SETCardDTO cardDTO) => new(cardDTO.Color, cardDTO.Shape, cardDTO.CountIndex, cardDTO.Shading);

        // The game scene is loaded before the join request is sent, so the UI is already listening when the game begins
        public void SignalGameLoaded()
        {
        }

        public void StartGuess()
        {
            connection.InvokeAsync(SETGameMessageNames.PlayerGuessStart);
        }

        public void RequestCardHint()
        {
            connection.InvokeAsync(SETGameMessageNames.CardHint);
        }

        public void GuessCards(IEnumerable<CardData> cards)
        {
            connection.InvokeAsync(SETGameMessageNames.PlayerGuess, new PlayerCardGuessRequest
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