using DG.Tweening;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using FunBoardGames.Network;

namespace FunBoardGames.Client
{
    public class LoginUIManager : MonoBehaviour
    {
        [SerializeField] TMP_Text messageLogText;
        [SerializeField] GameObject waitObject;
        [SerializeField] UserProfile userProfile;
        [SerializeField] SignUpDialog signUpDialog;

        INetworkManager networkManager;
        IAuthHandler authHandler;

        private void Start()
        {
            networkManager = NetworkSingleton.NetworkManager;
            networkManager.Connected += OnConnected;
            networkManager.ConnectionFailed += OnConnectionFailed;

            SetLoginProcess(true);
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
            SetLoginProcess(false);

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
            LogMessage(error);
            SetLoginProcess(false);
        }

        private void SetLoginProcess(bool isProcess)
        {
            waitObject.SetActive(isProcess);
        }

        private void OnLoginSuccess(SignInResponse _)
        {
            DOVirtual.DelayedCall(0.5f, () => SceneManager.LoadScene("Menu"));
        }

        private void OnLoginFailed(string errorMessage)
        {
            LogMessage(errorMessage);
            SetLoginProcess(false);
        }

        private void StartLogin()
        {
            SetLoginProcess(true);
            authHandler.SignIn(new SignInData{ PlayerName = PlayerPrefs.GetString("username"), DeviceId = Utilities.DeviceId, Password = PlayerPrefs.GetString("password") });
        }

        private void SignUp(string name)
        {
            SetLoginProcess(true);
            authHandler.SignUp(new SignUpData { PlayerName = name, DeviceId = Utilities.DeviceId });
        }

        private void LogMessage(string message, float duration = 3)
        {
            messageLogText.text = message;
            DOVirtual.DelayedCall(duration, () => messageLogText.text = "");
        }
    }
}
