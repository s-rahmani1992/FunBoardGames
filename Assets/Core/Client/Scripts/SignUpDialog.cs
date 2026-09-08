using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class SignUpDialog : BaseDialog
    {
        [SerializeField] TMPro.TMP_InputField usernameInputField;
        [SerializeField] Button chooseButton;

        public string ChosenUsername { get; private set; }

        public void OnChooseClicked()
        {
            ChosenUsername = usernameInputField.text;
            Close();
        }

        public void OnInputTextChanged(string text)
        {
            chooseButton.interactable = !string.IsNullOrWhiteSpace(text);
        }

        public void OnQuitClicked()
        {
            ChosenUsername = null;
            Application.Quit();
        }

        public override void Show()
        {
            base.Show();
            usernameInputField.text = "";
            chooseButton.interactable = false;
        }
    }
}