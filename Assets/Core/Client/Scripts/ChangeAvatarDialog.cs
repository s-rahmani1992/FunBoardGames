using FunBoardGames.Network;

using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class ChangeAvatarDialog : BaseDialog
    {
        [SerializeField] AvatarAssetManager avatarAssets;
        [SerializeField] UserProfile userProfile;
        [SerializeField] AvatarItem avatarItemPrefab;
        [SerializeField] Transform content;
        [SerializeField] ToggleGroup toggleGroup;
        [SerializeField] Button changeButton;

        IUserHandler userHandler;
        int currentAvatar;
        int selectedAvatar;

        public void OnChangeClicked()
        {
            userHandler.ChangeAvatar(selectedAvatar);
            changeButton.interactable = false;
        }

        public void OnCancelClicked()
        {
            Close();
        }

        public override void Show()
        {
            base.Show();
            userHandler = NetworkSingleton.NetworkManager.UserHandler;
            userHandler.ProfileUpdated += OnProfileUpdated;

            var profileAvatar = userProfile.Profile?.Avatar ?? 0;
            currentAvatar = profileAvatar >= 0 && profileAvatar < avatarAssets.Count ? profileAvatar : 0;
            selectedAvatar = currentAvatar;

            CreateItems();
            changeButton.interactable = false;
        }

        public override void OnClose()
        {
            if (userHandler != null)
                userHandler.ProfileUpdated -= OnProfileUpdated;
            base.OnClose();
        }

        private void CreateItems()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);

            for (int i = 0; i < avatarAssets.Count; i++)
            {
                var item = Instantiate(avatarItemPrefab, content);
                item.Initialize(i, avatarAssets.GetAvatar(i), toggleGroup);
                if (i == currentAvatar)
                    item.Select();
                item.Selected += OnItemSelected;
            }
        }

        private void OnItemSelected(AvatarItem item)
        {
            selectedAvatar = item.Index;
            changeButton.interactable = selectedAvatar != currentAvatar;
        }

        private void OnProfileUpdated(Profile _)
        {
            Close();
        }
    }
}
