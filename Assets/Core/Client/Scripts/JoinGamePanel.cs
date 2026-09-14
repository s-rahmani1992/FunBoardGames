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
        [SerializeField] RectTransform playersPanel;
        [SerializeField] RoomPlayerUI roomPlyerUI;
        [SerializeField] TMP_Text gameTxt;
        [SerializeField] TMP_Text logTxt;
        [SerializeField] GameObject waitingObject;
        [SerializeField] SceneAssetMap sceneMap;
        [SerializeField] Button leaveBtn;
        [SerializeField] UserGameHolder userGameHolder;

        Coroutine toast;
        ILobbyHandler lobbyHandler;
        IGameHandler gameHandler;
        BoardGame gameType;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            leaveBtn.onClick.AddListener(OnLeaveClicked);
        }

        private void OnLeaveClicked()
        {
            gameHandler?.LeaveGame();
            Destroy(gameObject);
        }

        public void Initialize(ILobbyHandler lobbyHandler, BoardGameData gameData)
        {
            this.lobbyHandler = lobbyHandler;
            this.gameType = gameData.Type;

            gameTxt.text = gameData.Name;
            waitingObject.SetActive(true);

            lobbyHandler.JoinedGame += OnJoinedGame;
            lobbyHandler.JoinGame(gameData.Id);
            userGameHolder.SetActiveGame(gameData);
        }

        private void OnJoinedGame(IGameHandler handler, IEnumerable<IBoardGamePlayer> players)
        {
            gameHandler = handler;
            waitingObject.SetActive(false);

            foreach (var p in players)
                OnPlayerAdded(p);

            gameHandler.PlayerJoined += OnPlayerJoined;
            gameHandler.AllPlayersReady += OnAllPlayersReady;
        }

        private void OnAllPlayersReady()
        {
            logTxt.text = "All players are ready. Loading game...";
            StartCoroutine(LoadGame());
        }

        private void OnPlayerAdded(IBoardGamePlayer player)
        {
            RoomPlayerUI uiPlayer = Instantiate(roomPlyerUI, playersPanel);
            uiPlayer.SetPlayer(player);
        }

        private void OnPlayerJoined(IBoardGamePlayer player)
        {
            OnPlayerAdded(player);
            Log($"{(player.IsMe ? "you" : player.Name)} Joined the game");
        }

        void Log(string str)
        {
            if (toast == null)
                toast = StartCoroutine(Toast(str));
            else
            {
                StopCoroutine(toast);
                toast = StartCoroutine(Toast(str));
            }
        }

        IEnumerator Toast(string str)
        {
            string currentTxt = logTxt.text;
            logTxt.text = str;
            yield return new WaitForSeconds(3);
            logTxt.text = currentTxt;
            toast = null;
        }

        void OnDestroy()
        {
            if (lobbyHandler != null)
                lobbyHandler.JoinedGame -= OnJoinedGame;
            if (gameHandler != null)
            {
                gameHandler.PlayerJoined -= OnPlayerJoined;
                gameHandler.AllPlayersReady -= OnAllPlayersReady;
            }
        }

        IEnumerator LoadGame()
        {
            yield return new WaitForSeconds(0.5f);
            yield return SceneManager.LoadSceneAsync(sceneMap.GetScene(gameType));
            yield return new WaitForSeconds(0.2f);
            Destroy(gameObject);
        }
    }
}
