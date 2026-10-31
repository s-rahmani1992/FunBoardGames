using DG.Tweening;
using FunBoardGames.Network;
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
        [SerializeField] UserProfileItem playerProfileItem;
        [SerializeField] UserProfileItem[] otherPlayerProfiles;
        [SerializeField] TMP_Text gameTxt;
        [SerializeField] TMP_Text logTxt;
        [SerializeField] SceneAssetMap sceneMap;
        [SerializeField] Button leaveBtn;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] UserProfile userProfile;

        ILobbyHandler lobbyHandler;
        IGameHandler gameHandler;
        BoardGame gameType;

        IBoardGamePlayer[] players;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            players = new IBoardGamePlayer[otherPlayerProfiles.Length];
            leaveBtn.interactable = false;
            leaveBtn.onClick.AddListener(OnLeaveClicked);
        }

        private void OnLeaveClicked()
        {
            gameHandler?.LeaveGame();
            Destroy(gameObject);
        }

        public void Initialize(ILobbyHandler lobbyHandler, GameData gameData)
        {
            this.lobbyHandler = lobbyHandler;
            this.gameType = gameData.Type;

            gameTxt.text = gameData.Name;

            playerProfileItem.SetProfile(userProfile.Profile);

            for (int i = 0; i < gameData.NumberofPlayers - 1; i++)
            {
                otherPlayerProfiles[i].gameObject.SetActive(true);
                otherPlayerProfiles[i].SetSearching();
            }

            lobbyHandler.JoinedGame += OnJoinedGame;
            userGameHolder.SetActiveGame(gameData);

            DOVirtual.DelayedCall(1, () => {
                leaveBtn.interactable = true;
                logTxt.text = "Searching for players...";
                lobbyHandler.JoinGame(gameData);
            });
        }

        private void OnJoinedGame(IGameHandler handler, IEnumerable<IBoardGamePlayer> players)
        {
            gameHandler = handler;

            foreach (var p in players)
                OnPlayerAdded(p);

            gameHandler.PlayerJoined += OnPlayerJoined;
            gameHandler.PlayerLeft += OnPlayerLeft;
            gameHandler.AllPlayersReady += OnAllPlayersReady;
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

        private void OnAllPlayersReady()
        {
            logTxt.text = "All players are ready. Loading game...";
            StartCoroutine(LoadGame());
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

        private void OnPlayerJoined(IBoardGamePlayer player)
        {
            OnPlayerAdded(player);
        }

        void OnDestroy()
        {
            if (lobbyHandler != null)
                lobbyHandler.JoinedGame -= OnJoinedGame;
            if (gameHandler != null)
            {
                gameHandler.PlayerJoined -= OnPlayerJoined;
                gameHandler.PlayerLeft -= OnPlayerLeft;
                gameHandler.AllPlayersReady -= OnAllPlayersReady;
            }
        }

        IEnumerator LoadGame()
        {
            leaveBtn.interactable = false;
            logTxt.text = "Loading game...";
            yield return new WaitForSeconds(0.5f);
            yield return SceneManager.LoadSceneAsync(sceneMap.GetScene(gameType));
            yield return new WaitForSeconds(0.2f);
            Destroy(gameObject);
        }
    }
}
