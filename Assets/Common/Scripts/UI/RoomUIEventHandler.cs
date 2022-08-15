using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OnlineBoardGames
{
    public class RoomUIEventHandler : MonoBehaviour
    {
        public Action<string> OnOtherPlayerJoined;
        public Action<string> OnOtherPlayerLeft;
        public Action OnLocalPlayerReady;
        public Action OnAllPlayersReady;
        public Action<bool> OnBeginStatChanged;

        private void Awake(){
            SingletonUIHandler.SetInstance(this);
        }

        private void OnDestroy(){
            OnOtherPlayerJoined = null;
            OnOtherPlayerLeft = null;
            OnLocalPlayerReady = null;
            OnAllPlayersReady = null;
            OnBeginStatChanged = null;
            SingletonUIHandler.SetInstance<MonoBehaviour>(null);
        }
    }
}