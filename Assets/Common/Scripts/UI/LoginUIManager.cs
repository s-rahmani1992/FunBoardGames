using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class LoginUIManager : MonoBehaviour
    {
        [SerializeField] InputField nameField;
        [SerializeField] Button loginBtn;
        [SerializeField] Text logTxt;
        [SerializeField] SimpleNameAuthenticator authenticator;

        private void Start()
        {
            authenticator.LoginFailed += OnLoginFailed;
            authenticator.LoginSuccess += OnLoginSuccess;
        }

        private void OnLoginSuccess()
        {
            SceneManager.LoadScene("Menu");
        }

        private void OnLoginFailed(string str)
        {
            logTxt.color = Color.red;
            logTxt.text = str;
            nameField.text = "";
            loginBtn.interactable = true;
        }

        private void OnLoginStarted()
        {
            logTxt.color = Color.yellow;
            logTxt.text = "Logging In";
            loginBtn.interactable = false;
        }

        public void UpdateBtn()
        {
            loginBtn.interactable = nameField.text.Length > 2;
        }

        public void AtemptLogin()
        {
            (GameNetworkManager.singleton.authenticator as SimpleNameAuthenticator).reqName = nameField.text;
            GameNetworkManager.singleton.StartClient();
            OnLoginStarted();
        }

        private void OnDestroy()
        {
            authenticator.LoginFailed -= OnLoginFailed;
            authenticator.LoginSuccess -= OnLoginSuccess;
        }
    }
}
