using FunBoardGames.Network;
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

        ILobbyHandler lobbyHandler;
        BoardGameData gameData;

        public void Initialize(BoardGameData gameData)
        {
            this.gameData = gameData;

            lobbyHandler = NetworkSingleton.NetworkManager.LobbyHandler;
            titleText.text = string.IsNullOrEmpty(gameData.Name) ? gameData.Type.ToString() : gameData.Name;
            playerCountText.text = $"{gameData.NumberofPlayers} Players";
            joinRandomButton.onClick.AddListener(OnJoinRandomClicked);
        }

        private void OnJoinRandomClicked()
        {
            var joinPanel = Instantiate(joinGamePanel);
            joinPanel.Initialize(lobbyHandler, gameData);
        }
    } 
}
