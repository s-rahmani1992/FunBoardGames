using FunBoardGames.Network;
using TMPro;
using UnityEngine;

namespace FunBoardGames.Client
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField] UserProfile userProfile;
        [SerializeField] TMP_Text playerNameText;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] TournamentGameHolder tournamentGameHolder;
        [SerializeField] GameBanner gameBannerPrefab;
        [SerializeField] RectTransform gameBannerHolder;

        ILobbyHandler lobbyHandler;

        // Start is called before the first frame update
        void Start()
        {
            lobbyHandler = NetworkSingleton.NetworkManager.LobbyHandler;
            playerNameText.text = userProfile.PlayerName;
            userGameHolder.Register(lobbyHandler);

            foreach (var gameData in tournamentGameHolder.Games)
            {
                var banner = Instantiate(gameBannerPrefab, gameBannerHolder);
                banner.Initialize(gameData);
            }
        }
    }
}