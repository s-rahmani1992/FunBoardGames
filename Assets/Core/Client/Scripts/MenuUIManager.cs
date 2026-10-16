using FunBoardGames.Network;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField] UserProfile userProfile;
        [SerializeField] AvatarAssetManager avatarAssets;
        [SerializeField] Image playerAvatarImage;
        [SerializeField] TMP_Text playerNameText;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] TournamentGameHolder tournamentGameHolder;
        [SerializeField] GameBanner gameBannerPrefab;
        [SerializeField] RectTransform gameBannerHolder;
        [SerializeField] ChangeNameDialog changeNameDialog;
        [SerializeField] ChangeAvatarDialog changeAvatarDialog;

        ILobbyHandler lobbyHandler;
        IUserHandler userHandler;

        // Start is called before the first frame update
        void Start()
        {
            lobbyHandler = NetworkSingleton.NetworkManager.LobbyHandler;
            userHandler = NetworkSingleton.NetworkManager.UserHandler;
            playerNameText.text = userProfile.PlayerName;
            playerAvatarImage.sprite = avatarAssets.GetAvatar(userProfile.Profile.Avatar);
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
            playerAvatarImage.sprite = avatarAssets.GetAvatar(profile.Avatar);
        }

        public void OnChangeNameClicked()
        {
            DialogManager.Instance.ShowDialog(changeNameDialog, DialogShowOptions.OverAll);
        }

        public void OnChangeAvatarClicked()
        {
            DialogManager.Instance.ShowDialog(changeAvatarDialog, DialogShowOptions.OverAll);
        }
    }
}