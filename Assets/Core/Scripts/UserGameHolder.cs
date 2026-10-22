using FunBoardGames.Network;
using UnityEngine;

public class UserGameHolder : ScriptableObject
{
    IGameHandler _currentGameHandler;
    ILobbyHandler _lobbyHandler;

    public BoardGameData ActiveGame { get; private set; }

    public bool IsTutorial { get; private set; }

    public void SetActiveGame(BoardGameData gameData)
    {
        EndTutorial();
        ActiveGame = gameData;
    }

    public void StartTutorial(BoardGameData gameData, IGameHandler tutorialHandler)
    {
        if (_currentGameHandler != null)
            _currentGameHandler.PlayerLeft -= OnPlayerLeft;

        ActiveGame = gameData;
        _currentGameHandler = tutorialHandler;
        IsTutorial = true;
    }

    public void EndTutorial()
    {
        if (IsTutorial == false)
            return;

        IsTutorial = false;
        ActiveGame = null;
        _currentGameHandler = null;
    }

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
