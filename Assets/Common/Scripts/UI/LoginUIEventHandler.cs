using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OnlineBoardGames
{
    public class LoginUIEventHandler : MonoBehaviour
    {
        public Action<string> OnLoginError;
        public Action OnLoginSuccess;
        public Action OnLoginStarted;

        private void Awake(){
            SingletonUIHandler.SetInstance(this);
        }

        private void OnDestroy(){
            OnLoginSuccess = null;
            OnLoginStarted = null;
            OnLoginError = null;
            SingletonUIHandler.SetInstance<MonoBehaviour>(null);
        }
    }
}