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
        public event Action<ISETPlayer> PlayerStartedGuess;
        public event Action<ISETPlayer> PlayerGuessTimeout;
        public event Action<ISETPlayer, IEnumerable<CardData>, bool> PlayerGuessReceived;
        public event Action<ISETPlayer> PlayerRequestCards;
        public event Action<ISETPlayer, bool> PlayerVoteReceived;
        public event Action<bool> VoteResultReceived;

        List<SignalRSETPlayer> playerList = new();
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

            connectionHooks.Add(_connection.On<DistributeNewCardsMessage>(SETGameMessageNames.DistributeCards, (cardMsg) =>
            {
                unityContext.Post(_ => OnNewCardsReceived(cardMsg), null);
            }));

            connectionHooks.Add(_connection.On<PlayerGuessStartMessage>(SETGameMessageNames.PlayerGuessStart, (guessMsg) =>
            {
                unityContext.Post(_ => OnPlayerStartedGuess(guessMsg), null);
            }));

            connectionHooks.Add(_connection.On<GuessResultResponse>(SETGameMessageNames.PlayerGuess, (guessMsg) =>
            {
                unityContext.Post(_ => OnPlayerGuessResultReceived(guessMsg), null);
            }));

            connectionHooks.Add(_connection.On<PlayerStartedVoteResponse>(SETGameMessageNames.PlayerStartCardVote, (startVoteMsg) =>
            {
                unityContext.Post(_ => OnPlayerStartVoteReceived(startVoteMsg), null);
            }));

            connectionHooks.Add(_connection.On<PlayerVoteResponse>(SETGameMessageNames.PlayerCardVote, (voteMsg) =>
            {
                unityContext.Post(_ => OnPlayerVoteReceived(voteMsg), null);
            }));

            connectionHooks.Add(_connection.On<VoteResultResponse>(SETGameMessageNames.CardVoteResult, (voteResultMsg) =>
            {
                unityContext.Post(_ => OnVoteResultReceived(voteResultMsg), null);
            }));
        }

        private void OnVoteResultReceived(VoteResultResponse voteResultMsg)
        {
            VoteResultReceived?.Invoke(voteResultMsg.VotePassed);
        }

        private void OnPlayerVoteReceived(PlayerVoteResponse voteMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == voteMsg.ConnectionId);
            player.SetVote(voteMsg.IsVoteYes);
            PlayerVoteReceived?.Invoke(player, voteMsg.IsVoteYes);
        }

        private void OnPlayerStartVoteReceived(PlayerStartedVoteResponse startVoteMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == startVoteMsg.ConnectionId);
            player.SetVote(true);
            PlayerRequestCards?.Invoke(player);
        }

        private void OnPlayerGuessResultReceived(GuessResultResponse guessMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == guessMsg.ConnectionId);

            if (guessMsg.GuessedCards == null)
            {
                player.SetWrongScore(guessMsg.WrongScore);
                PlayerGuessTimeout?.Invoke(player);
                return;
            }

            player.SetCorrectScore(guessMsg.CorrectScore);
            player.SetWrongScore(guessMsg.WrongScore);
            PlayerGuessReceived?.Invoke(player, guessMsg.GuessedCards.Select(cardDTO => new CardData(cardDTO.Color, cardDTO.Shape, cardDTO.CountIndex, cardDTO.Shading)), guessMsg.GuessedCorrect);
        }

        private void OnPlayerStartedGuess(PlayerGuessStartMessage guessMsg)
        {
            SignalRSETPlayer player = playerList.FirstOrDefault(player => player.ConnectionId == guessMsg.ConnectionId);
            PlayerStartedGuess?.Invoke(player);
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
            _connection.InvokeAsync(SETGameMessageNames.GameLoaded);
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

        public void RequestMoreCards()
        {
            _connection.SendAsync(SETGameMessageNames.PlayerStartCardVote);
        }

        public void VoteCard(bool positive)
        {
            _connection.SendAsync(SETGameMessageNames.PlayerCardVote, new PlayerVoteRequest()
            {
                Vote = positive
            });
        }
    }
}