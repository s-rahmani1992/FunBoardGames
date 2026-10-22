using FunBoardGames.Network;
using FunBoardGames.SET;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client 
{
    public class GameBanner : MonoBehaviour
    {
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text playerCountText;
        [SerializeField] Button joinRandomButton;
        [SerializeField] JoinGamePanel joinGamePanel;
        [SerializeField] Button howToPlayButton;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] UserProfile userProfile;
        [SerializeField] SceneAssetMap sceneMap;

        ILobbyHandler lobbyHandler;
        BoardGameData gameData;

        public void Initialize(BoardGameData gameData)
        {
            this.gameData = gameData;

            lobbyHandler = NetworkSingleton.NetworkManager.LobbyHandler;
            titleText.text = string.IsNullOrEmpty(gameData.Name) ? gameData.Type.ToString() : gameData.Name;
            playerCountText.text = $"{gameData.NumberofPlayers} Players";
            joinRandomButton.onClick.AddListener(OnJoinRandomClicked);

            howToPlayButton.gameObject.SetActive(gameData.Type == BoardGame.SET);
            howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
        }

        private void OnHowToPlayClicked()
        {
            SETTutorialFlow.Launch(userGameHolder, userProfile.Profile, sceneMap.GetScene(BoardGame.SET));
        }

        private void OnJoinRandomClicked()
        {
            var joinPanel = Instantiate(joinGamePanel);
            joinPanel.Initialize(lobbyHandler, gameData);
        }
    } 
}
