using DG.Tweening;
using FishNet.Managing;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames.UI
{
    public class LoginUIManager : MonoBehaviour
    {
        [SerializeField] InputField nameField;
        [SerializeField] Button loginBtn;
        [SerializeField] Text logTxt;

        NetworkManager networkManager;

        private void Start()
        {
            networkManager = NetworkManager.Instances.ElementAt(0);
            networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
            networkManager.ClientManager.RegisterBroadcast<AuthResponseMessage>(OnAuthResponseMessage);
            networkManager.ClientManager.RegisterBroadcast<AuthSyncMessage>(OnAuthSynced);
        }

        private void OnAuthSynced(AuthSyncMessage message)
        {
            networkManager.ClientManager.Connection.CustomData = message.authData;
        }

        void OnAuthResponseMessage(AuthResponseMessage msg)
        {
            if (msg.resultCode != 0)
                OnLoginFailed(msg.message);
            else
                OnLoginSuccess();
        }

        private void OnClientConnectionState(FishNet.Transporting.ClientConnectionStateArgs e)
        {
            DebugStep.Log($"Client Connection->{e.ConnectionState}");
            if (e.ConnectionState == FishNet.Transporting.LocalConnectionState.Started)
            {
                networkManager.ClientManager.Broadcast(new AuthRequestMessage { requestedName = nameField.text });
                return;
            }
        }

        private void OnLoginSuccess()
        {
            DOVirtual.DelayedCall(0.5f, () => SceneManager.LoadScene("Menu"));
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
            networkManager.ClientManager.StartConnection();
            OnLoginStarted();
        }

        private void OnDestroy()
        {
            networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            networkManager.ClientManager.UnregisterBroadcast<AuthResponseMessage>(OnAuthResponseMessage);
            networkManager.ClientManager.UnregisterBroadcast<AuthSyncMessage>(OnAuthSynced);
        }
    }
}
