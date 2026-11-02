using FunBoardGames.Network;
using UnityEngine;

public class UserGameHolder : ScriptableObject
{
    IGameHandler _currentGameHandler;

    public GameData ActiveGame { get; private set; }

    public bool IsTutorial { get; private set; }

    public void SetActiveGame(GameData gameData, IGameHandler gameHandler)
    {
        EndTutorial();
        ReleaseHandler();

        ActiveGame = gameData;
        _currentGameHandler = gameHandler;
        _currentGameHandler.LeftGame += OnLeftGame;
    }

    public void StartTutorial(GameData gameData, IGameHandler tutorialHandler)
    {
        ReleaseHandler();

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

    private void OnLeftGame()
    {
        ReleaseHandler();
        ActiveGame = null;
    }

    void ReleaseHandler()
    {
        if (_currentGameHandler == null)
            return;

        _currentGameHandler.LeftGame -= OnLeftGame;
        _currentGameHandler.Dispose();
        _currentGameHandler = null;
    }

    public T GetGameHandler<T>() where T : IGameHandler
    {
        return (T)_currentGameHandler;
    }

    private void OnDisable()
    {
        ReleaseHandler();
    }
}
