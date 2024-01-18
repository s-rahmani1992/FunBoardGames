using DG.Tweening;
using TMPro;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Transporting;

namespace OnlineBoardGames.Client
{
    public class LoginUIManager : MonoBehaviour
    {
        [SerializeField] TMP_InputField nameField;
        [SerializeField] Button loginButton;
        [SerializeField] TMP_Text messageLogText;
        [SerializeField] GameObject waitObject;

        ClientManager clientManager;

        private void Start()
        {
            clientManager = NetworkManager.Instances.ElementAt(0).ClientManager;
            clientManager.OnClientConnectionState += OnClientConnectionState;
            clientManager.RegisterBroadcast<AuthResponseMessage>(OnAuthResponseMessage);
            loginButton.onClick.AddListener(StartLogin);
            nameField.onValueChanged.AddListener(OnNameFieldChanged);
            OnNameFieldChanged(nameField.text);
            SetLoginProcess(false);

            if (ClientDataManager.CheckDisconnectFlag())
                LogMessage("Disconnected");
        }

        private void OnDestroy()
        {
            clientManager.OnClientConnectionState -= OnClientConnectionState;
            clientManager.UnregisterBroadcast<AuthResponseMessage>(OnAuthResponseMessage);
        }

        private void OnAuthResponseMessage(AuthResponseMessage msg)
        {
            if (msg.resultCode != 0)
                OnLoginFailed(msg.message);
            else
                OnLoginSuccess();
        }

        private void SetLoginProcess(bool isProcess)
        {
            loginButton.gameObject.SetActive(!isProcess);
            nameField.interactable = !isProcess;
            waitObject.SetActive(isProcess);
        }

        private void OnClientConnectionState(ClientConnectionStateArgs e)
        {
            if (e.ConnectionState == LocalConnectionState.Started)
            {
                clientManager.Broadcast(new AuthRequestMessage { requestedName = nameField.text });
                return;
            }
            if (e.ConnectionState == LocalConnectionState.Stopped)
            {
                OnLoginFailed("Connection Failed");
                return;
            }
        }

        private void OnLoginSuccess()
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
            clientManager.StartConnection();
            SetLoginProcess(true);
        }

        private void LogMessage(string message, float duration = 3)
        {
            messageLogText.text = message;
            nameField.text = "";
            DOVirtual.DelayedCall(duration, () => messageLogText.text = "");
        }
    }
}
