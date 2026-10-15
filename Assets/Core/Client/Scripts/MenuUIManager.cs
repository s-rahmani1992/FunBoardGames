using FunBoardGames.Network;
using System;
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
        [SerializeField] ChangeNameDialog changeNameDialog;

        ILobbyHandler lobbyHandler;
        IUserHandler userHandler;

        // Start is called before the first frame update
        void Start()
        {
            lobbyHandler = NetworkSingleton.NetworkManager.LobbyHandler;
            userHandler = NetworkSingleton.NetworkManager.UserHandler;
            playerNameText.text = userProfile.PlayerName;
            userGameHolder.Register(lobbyHandler);

            userHandler.ProfileUpdated += OnProfileUpdated;

            foreach (var gameData in tournamentGameHolder.Games)
            {
                var banner = Instantiate(gameBannerPrefab, gameBannerHolder);
                banner.Initialize(gameData);
            }
        }

        private void OnDestroy()
        {
            userHandler.ProfileUpdated -= OnProfileUpdated;
        }

        private void OnProfileUpdated(Profile profile)
        {
            playerNameText.text = profile.PlayerName;
        }

        public void OnChangeNameClicked()
        {
            DialogManager.Instance.ShowDialog(changeNameDialog, DialogShowOptions.OverAll);
        }
    }
}