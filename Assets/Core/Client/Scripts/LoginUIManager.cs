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
        [SerializeField] TMP_InputField nameField;
        [SerializeField] Button loginButton;
        [SerializeField] TMP_Text messageLogText;
        [SerializeField] GameObject waitObject;
        [SerializeField] UserProfile userProfile;

        IAuthHandler authHandler;

        private void Start()
        {
            authHandler = NetworkSingleton.NetworkManager.AuthHandler;
            userProfile.Register(authHandler);
            authHandler.LoginSuccess += OnLoginSuccess;
            authHandler.LoginFailed += OnLoginFailed;
            loginButton.onClick.AddListener(StartLogin);
            nameField.onValueChanged.AddListener(OnNameFieldChanged);
            OnNameFieldChanged(nameField.text);
            SetLoginProcess(false);
        }

        private void OnDestroy()
        {
            authHandler.LoginSuccess -= OnLoginSuccess;
            authHandler.LoginFailed -= OnLoginFailed;
        }

        private void SetLoginProcess(bool isProcess)
        {
            loginButton.gameObject.SetActive(!isProcess);
            nameField.interactable = !isProcess;
            waitObject.SetActive(isProcess);
        }

        private void OnLoginSuccess(Profile _)
        {
            DOVirtual.DelayedCall(0.5f, () => SceneManager.LoadScene("Menu"));
        }

        private void OnLoginFailed(string errorMessage)
        {
            LogMessage(errorMessage);
            nameField.text = "";
            SetLoginProcess(false);
        }

        private void OnNameFieldChanged(string text)
        {
            loginButton.interactable = text.Length > 2;
        }

        private void StartLogin()
        {
            SetLoginProcess(true);
            authHandler.Authenticate(nameField.text);
        }

        private void LogMessage(string message, float duration = 3)
        {
            messageLogText.text = message;
            nameField.text = "";
            DOVirtual.DelayedCall(duration, () => messageLogText.text = "");
        }
    }
}
