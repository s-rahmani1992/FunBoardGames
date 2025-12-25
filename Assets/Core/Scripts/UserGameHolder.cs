using FunBoardGames.Network;
using UnityEngine;

public class UserGameHolder : ScriptableObject
{
    IGameHandler _currentGameHandler;
    ILobbyHandler _lobbyHandler;

    public void Register(ILobbyHandler lobbyHandler)
    {
        if (_lobbyHandler != null)
            return;

        _lobbyHandler = lobbyHandler;

        lobbyHandler.JoinedGame += OnJoinedGame;
    }

    private void OnJoinedGame(IGameHandler gameHandler, System.Collections.Generic.IEnumerable<IBoardGamePlayer> _)
    {
        _currentGameHandler = gameHandler;
        _currentGameHandler.PlayerLeft += OnPlayerLeft;
    }

    private void OnPlayerLeft(IBoardGamePlayer player)
    {
        if (player.IsMe)
        {
            _currentGameHandler.PlayerLeft -= OnPlayerLeft;
            _currentGameHandler = null;
        }
    }

    public T GetGameHandler<T>() where T : IGameHandler
    {
        return (T)_currentGameHandler;
    }

    private void OnDisable()
    {
        if(_lobbyHandler != null)
            _lobbyHandler.JoinedGame -= OnJoinedGame;
    }
}
