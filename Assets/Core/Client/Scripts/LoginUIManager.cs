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

        //TODO : This is a temporary solution to pass the game data to the menu scene. Replace it with scriptable object later
        public static List<BoardGameData> Games { get; private set; }
        public static BoardGameData ActiveGame { get; set; }

        INetworkManager networkManager;
        IAuthHandler authHandler;
        IUserHandler userHandler;

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

            if (userHandler != null)
            {
                userHandler.UserGameDataReceived -= OnUserGameDataReceived;
            }
        }

        private void OnConnected()
        {
            SetProgress(0.3f, "Signing In .....");

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
            SetProgress(0.6f, "Getting Data .....");
            userHandler = networkManager.UserHandler;
            userHandler.UserGameDataReceived += OnUserGameDataReceived;
            userHandler.GetUserData();
        }

        private void OnUserGameDataReceived(UserGameData obj)
        {
            SetProgress(1.0f, "Loading Menu .....");
            Games = obj.Games;
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
            authHandler.SignIn(new SignInData{ PlayerName = PlayerPrefs.GetString("username"), DeviceId = Utilities.DeviceId, Password = PlayerPrefs.GetString("password") });
        }

        private void SignUp(string name)
        {
            SetProgress(0.5f, "Signing In .....");
            authHandler.SignUp(new SignUpData { PlayerName = name, DeviceId = Utilities.DeviceId });
        }
    }
}
