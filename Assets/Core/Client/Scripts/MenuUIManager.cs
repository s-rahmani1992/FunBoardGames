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
        [SerializeField] ChangeNameDialog changeNameDialog;
        [SerializeField] ChangeAvatarDialog changeAvatarDialog;

        IUserHandler userHandler;

        // Start is called before the first frame update
        void Start()
        {
            userHandler = NetworkSingleton.NetworkManager.UserHandler;
            playerNameText.text = userProfile.PlayerName;
            playerAvatarImage.sprite = avatarAssets.GetAvatar(userProfile.Profile.Avatar);

            userHandler.ProfileUpdated += OnProfileUpdated;
        }

        private void OnDestroy()
        {
            userHandler.ProfileUpdated -= OnProfileUpdated;
        }

        private void OnProfileUpdated(ProfileData profile)
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