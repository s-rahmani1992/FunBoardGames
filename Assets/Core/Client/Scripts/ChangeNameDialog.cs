using UnityEngine;
using FunBoardGames.Network;

using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class ChangeNameDialog : BaseDialog
    {
        [UnityEngine.SerializeField] TMPro.TMP_InputField nameInputField;
        [UnityEngine.SerializeField] Button changeButton;
        [SerializeField]  UserProfile userProfile;

        IUserHandler userHandler;

        public void OnChangeClicked()
        {
            var newName = nameInputField.text.Trim();
            if (string.IsNullOrEmpty(newName)) return;

            userHandler.ChangePlayerName(newName);
            changeButton.interactable = false;
        }

        public void OnInputTextChanged(string text)
        {
            changeButton.interactable = text != userProfile.PlayerName;
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
            nameInputField.text = "";
            changeButton.interactable = false;
            nameInputField.text = userProfile.PlayerName;
        }

        public override void OnClose()
        {
            userHandler.ProfileUpdated -= OnProfileUpdated;
            base.OnClose();
        }

        private void OnProfileUpdated(Profile _)
        {
            Close();
        }
    }
}
