using DG.Tweening;
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
    public abstract class JoinGamePanel<ITGameHandler, TPlayer> : MonoBehaviour
        where TPlayer : IBoardGamePlayer
        where ITGameHandler : IGameHandlerT<TPlayer>
    {
        const string MenuScene = "Menu";
        const float LeaveButtonDelay = 5f;
        const float JoinFailedDelay = 1.5f;
        const float CloseAnimationDuration = 0.35f;
        static readonly int CloseTrigger = Animator.StringToHash("close");

        [SerializeField] UserProfileItem playerProfileItem;
        [SerializeField] UserProfileItem[] otherPlayerProfiles;
        [SerializeField] TMP_Text gameTxt;
        [SerializeField] TMP_Text logTxt;
        [SerializeField] SceneAssetMap sceneMap;
        [SerializeField] Button leaveBtn;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] UserProfile userProfile;
        [SerializeField] Animator animator;

        ITGameHandler gameHandler;
        Action unsubscribe;
        GameData gameData;

        TPlayer[] players;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            players = new TPlayer[otherPlayerProfiles.Length];
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

            gameHandler = NetworkSingleton.NetworkManager.CreateGameHandler<ITGameHandler>();
            Subscribe(gameHandler);
            userGameHolder.SetActiveGame(gameData, gameHandler);
            StartCoroutine(LoadGameAndJoin());
        }

        void Subscribe(ITGameHandler handler)
        {
            Action<IEnumerable<TPlayer>> joinedGame = OnJoinedGame;
            Action<TPlayer> playerJoined = OnPlayerAdded;
            Action<TPlayer> playerLeft = OnPlayerLeft;

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

        IEnumerator LoadGameAndJoin()
        {
            logTxt.text = "Searching for players...";
            yield return SceneManager.LoadSceneAsync(sceneMap.GetScene(gameData.Type));
            yield return new WaitForSeconds(1);
            gameHandler.JoinGame(gameData);
            yield return EnableLeaveButton();
        }

        void OnJoinedGame(IEnumerable<TPlayer> joinedPlayers)
        {
            foreach (var p in joinedPlayers)
                OnPlayerAdded(p);
        }

        IEnumerator EnableLeaveButton()
        {
            yield return new WaitForSeconds(LeaveButtonDelay);
            leaveBtn.interactable = true;
        }

        void OnLeaveClicked()
        {
            leaveBtn.interactable = false;
            logTxt.text = "Leaving...";
            gameHandler.LeaveGame();
        }

        void OnLeftGame()
        {
            StartCoroutine(LoadMenuAndClose(0f));
        }

        void OnJoinFailed()
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
            animator.SetTrigger(CloseTrigger);
            Destroy(gameObject, CloseAnimationDuration);
        }

        void OnGameBegan()
        {
            leaveBtn.interactable = false;
            logTxt.text = "Loading game...";
            StopAllCoroutines();
            DOVirtual.DelayedCall(2.0f, () => animator.SetTrigger(CloseTrigger));
            Destroy(gameObject, 2.5f);
        }

        void OnPlayerLeft(TPlayer player)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (ReferenceEquals(players[i], player))
                {
                    players[i] = default;
                    otherPlayerProfiles[i].SetSearching();
                    break;
                }
            }
        }

        void OnPlayerAdded(TPlayer player)
        {
            if (player.IsMe)
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
