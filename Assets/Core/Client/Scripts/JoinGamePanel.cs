using FunBoardGames.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class JoinGamePanel : MonoBehaviour
    {
        const string MenuScene = "Menu";
        const float LeaveButtonDelay = 5f;
        const float JoinFailedDelay = 1.5f;

        [SerializeField] UserProfileItem playerProfileItem;
        [SerializeField] UserProfileItem[] otherPlayerProfiles;
        [SerializeField] TMP_Text gameTxt;
        [SerializeField] TMP_Text logTxt;
        [SerializeField] SceneAssetMap sceneMap;
        [SerializeField] Button leaveBtn;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] UserProfile userProfile;

        IGameHandler gameHandler;
        Action unsubscribe;
        GameData gameData;

        IBoardGamePlayer[] players;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            players = new IBoardGamePlayer[otherPlayerProfiles.Length];
            leaveBtn.interactable = false;
            leaveBtn.onClick.AddListener(OnLeaveClicked);
        }

        public void Initialize(GameData gameData)
        {
            this.gameData = gameData;
            gameTxt.text = gameData.Name;
            playerProfileItem.SetProfile(userProfile.Profile);

            for (int i = 0; i < gameData.NumberofPlayers - 1; i++)
            {
                otherPlayerProfiles[i].gameObject.SetActive(true);
                otherPlayerProfiles[i].SetSearching();
            }

            gameHandler = CreateGameHandler(gameData.Type);
            userGameHolder.SetActiveGame(gameData, gameHandler);
            StartCoroutine(LoadGameAndJoin());
        }

        IEnumerator LoadGameAndJoin()
        {
            logTxt.text = "Searching for players...";
            yield return SceneManager.LoadSceneAsync(sceneMap.GetScene(gameData.Type));
            yield return new WaitForSeconds(1);
            gameHandler.JoinGame(gameData);
            yield return EnableLeaveButton();
        }

        IGameHandler CreateGameHandler(BoardGame gameType)
        {
            var networkManager = NetworkSingleton.NetworkManager;

            switch (gameType)
            {
                case BoardGame.SET:
                    var setHandler = networkManager.CreateGameHandler<ISETGameHandler>();
                    Subscribe(setHandler);
                    return setHandler;
                case BoardGame.CantStop:
                    var cantStopHandler = networkManager.CreateGameHandler<ICantStopGameHandler>();
                    Subscribe(cantStopHandler);
                    return cantStopHandler;
                default:
                    throw new NotSupportedException($"{gameType} cannot be joined.");
            }
        }

        void Subscribe<TPlayer>(IGameHandlerT<TPlayer> handler) where TPlayer : IBoardGamePlayer
        {
            Action<IEnumerable<TPlayer>> joinedGame = OnJoinedGame;
            Action<TPlayer> playerJoined = player => OnPlayerAdded(player);
            Action<TPlayer> playerLeft = player => OnPlayerLeft(player);

            handler.JoinedGame += joinedGame;
            handler.PlayerJoined += playerJoined;
            handler.PlayerLeft += playerLeft;
            handler.JoinFailed += OnJoinFailed;
            handler.GameBegan += OnGameBegan;
            handler.LeftGame += OnLeftGame;

            unsubscribe = () =>
            {
                handler.JoinedGame -= joinedGame;
                handler.PlayerJoined -= playerJoined;
                handler.PlayerLeft -= playerLeft;
                handler.JoinFailed -= OnJoinFailed;
                handler.GameBegan -= OnGameBegan;
                handler.LeftGame -= OnLeftGame;
            };
        }

        private void OnJoinedGame<TPlayer>(IEnumerable<TPlayer> joinedPlayers) where TPlayer : IBoardGamePlayer
        {
            foreach (var p in joinedPlayers)
                OnPlayerAdded(p);
        }

        IEnumerator EnableLeaveButton()
        {
            yield return new WaitForSeconds(LeaveButtonDelay);
            leaveBtn.interactable = true;
        }

        private void OnLeaveClicked()
        {
            leaveBtn.interactable = false;
            logTxt.text = "Leaving...";
            gameHandler.LeaveGame();
        }

        private void OnLeftGame()
        {
            StartCoroutine(LoadMenuAndClose(0f));
        }

        private void OnJoinFailed()
        {
            logTxt.text = "Unable to join the game.";
            StartCoroutine(LoadMenuAndClose(JoinFailedDelay));
        }

        IEnumerator LoadMenuAndClose(float delay)
        {
            leaveBtn.interactable = false;

            if (delay > 0)
                yield return new WaitForSeconds(delay);

            yield return SceneManager.LoadSceneAsync(MenuScene);
            Destroy(gameObject);
        }

        private void OnGameBegan()
        {
            leaveBtn.interactable = false; 
            logTxt.text = "Loading game...";
            StopAllCoroutines();
            Destroy(gameObject, 2);
        }

        private void OnPlayerLeft(IBoardGamePlayer player)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if(players[i] == player)
                {
                    players[i] = null;
                    otherPlayerProfiles[i].SetSearching();
                    break;
                }
            }
        }

        private void OnPlayerAdded(IBoardGamePlayer player)
        {
            if(player.IsMe)
                return;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                    continue;

                players[i] = player;
                otherPlayerProfiles[i].SetProfile(player.Profile);
                break;
            }
        }

        void OnDestroy()
        {
            unsubscribe?.Invoke();
        }
    }
}
