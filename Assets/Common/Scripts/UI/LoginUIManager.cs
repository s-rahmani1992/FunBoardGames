using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class LoginUIManager : MonoBehaviour
    {
        [SerializeField]
        InputField nameField;
        [SerializeField]
        Button loginBtn;
        [SerializeField]
        Text logTxt;

        private void Start(){
            var eventHandler = SingletonUIHandler.GetInstance<LoginUIEventHandler>();
            eventHandler.OnLoginStarted += OnLoginStarted;
            eventHandler.OnLoginError += OnLoginFailed;
            eventHandler.OnLoginError += OnLoginFailed;
        }

        private void OnLoginFailed(string str){
            logTxt.color = Color.red;
            logTxt.text = str;
            nameField.text = "";
            loginBtn.interactable = true;
        }

        private void OnLoginStarted(){
            logTxt.color = Color.yellow;
            logTxt.text = "Logging In";
            loginBtn.interactable = false;
        }

        public void UpdateBtn(){
            loginBtn.interactable = nameField.text.Length > 2;
        }

        public void AtemptLogin(){
            (BoardGameNetworkManager.singleton.authenticator as SimpleNameAuthenticator).reqName = nameField.text;
            BoardGameNetworkManager.singleton.StartClient();
            SingletonUIHandler.GetInstance<LoginUIEventHandler>().OnLoginStarted?.Invoke();
        }
    }
}
