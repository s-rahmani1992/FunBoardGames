using DG.Tweening;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using FunBoardGames.Network;

namespace FunBoardGames.Client
{
    public class LoginUIManager : MonoBehaviour
    {
        [SerializeField] UserProfile userProfile;
        [SerializeField] SignUpDialog signUpDialog;
        [SerializeField] MessageDialog messageDialog;
        [SerializeField] Image progressFillImage;
        [SerializeField] TMP_Text progressText;

        INetworkManager networkManager;
        IAuthHandler authHandler;

        private void Start()
        {
            networkManager = NetworkSingleton.NetworkManager;
            networkManager.Connected += OnConnected;
            networkManager.ConnectionFailed += OnConnectionFailed;

            SetProgress(0f, "Connecting ....");
            networkManager.Connect();
        }

        private void OnDestroy()
        {
            networkManager.Connected -= OnConnected;
            networkManager.ConnectionFailed -= OnConnectionFailed;

            if (authHandler != null)
            {
                authHandler.LoginSuccess -= OnLoginSuccess;
                authHandler.LoginFailed -= OnLoginFailed;
            }
        }

        private void OnConnected()
        {
            authHandler = networkManager.AuthHandler;
            userProfile.Register(authHandler);
            authHandler.LoginSuccess += OnLoginSuccess;
            authHandler.LoginFailed += OnLoginFailed;

            if(PlayerPrefs.HasKey("username"))
            {
                StartLogin();
            }
            else
            {
                var dialog = DialogManager.Instance.ShowDialog(signUpDialog, DialogShowOptions.OverAll);
                dialog.OnClosedEvent += () =>
                {
                   SignUp(dialog.ChosenUsername);
                };
            }
        }

        private void OnConnectionFailed(string error)
        {
            var data = new MessageData("Error", error, new List<ButtonData>
            {
                new ButtonData("Retry", OnRetryConnectionClicked),
                new ButtonData("Quit", OnQuitClicked)
            });
            DialogManager.Instance.ShowDialog(messageDialog, DialogShowOptions.OverAll, data);
        }

        private void OnRetryConnectionClicked()
        {
            SetProgress(0f, "Connecting ....");
            networkManager.Connect();
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }

        private void SetProgress(float fillAmount, string text)
        {
            progressFillImage.fillAmount = fillAmount;
            progressText.text = text;
        }

        private void OnLoginSuccess(SignInResponse _)
        {
            SetProgress(1f, "Loading .....");
            DOVirtual.DelayedCall(0.5f, () => SceneManager.LoadScene("Menu"));
        }

        private void OnLoginFailed(string errorMessage)
        {
            MessageData data = new MessageData("Error", errorMessage, new List<ButtonData>
            {
                new ButtonData("Retry", StartLogin),
                new ButtonData("Quit", OnQuitClicked)
            });
            DialogManager.Instance.ShowDialog(messageDialog, DialogShowOptions.OverAll, data);
        }

        private void StartLogin()
        {
            SetProgress(0.5f, "Signing In .....");
            authHandler.SignIn(new SignInData{ PlayerName = PlayerPrefs.GetString("username"), DeviceId = Utilities.DeviceId, Password = PlayerPrefs.GetString("password") });
        }

        private void SignUp(string name)
        {
            SetProgress(0.5f, "Signing In .....");
            authHandler.SignUp(new SignUpData { PlayerName = name, DeviceId = Utilities.DeviceId });
        }
    }
}
